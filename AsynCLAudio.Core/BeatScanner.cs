using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsynCLAudio.Core
{
	public static class BeatScanner
	{
		public static double LowCutoff { get; set; } = 2.0;
		public static double HighCutoff { get; set; } = 5.0;


		public static async Task<double> ScanBpmAsync(AudioObj obj, int windowSize = 65536, int lookingRange = 2, int minBpm = 60, int maxBpm = 200)
		{
			if (obj == null || obj.Data == null || obj.Data.Length <= 0)
			{
				return -1.0f;
			}

			var monoData = await obj.GetCurrentWindow(windowSize, lookingRange, true);

			return await EstimateBpmAsync(monoData, obj.SampleRate, minBpm, maxBpm);
		}

		public static async Task<double> EstimateBpmAsync(float[] samples, int sampleRate, int minBpm = 60, int maxBpm = 200)
		{
			if (samples == null || samples.Length == 0 || sampleRate <= 0 || minBpm <= 0 || maxBpm <= minBpm)
			{
				return 0.0;
			}

			return await Task.Run(() =>
			{
				int n = samples.Length;

				// 1) Vorverarbeitung: Hüllkurve/Onset-ähnliches Signal
				//    Absolutwert -> schnelle & langsame gleitende Mittelwerte -> Halbwellendetektion
				double[] abs = new double[n];
				for (int i = 0; i < n; i++)
				{
					abs[i] = Math.Abs(samples[i]);
				}

				// Fenster für schnelle/slow MA (10ms/400ms, gekappt auf Datenlänge)
				int fastWin = Math.Clamp(sampleRate / 100, 1, n);   // ~10 ms
				int slowWin = Math.Clamp(sampleRate / 2, fastWin + 1, n); // ~0.5 s

				double[] fast = new double[n];
				double[] slow = new double[n];

				// Rolling sums für O(n) Moving Average
				double sumFast = 0, sumSlow = 0;
				for (int i = 0; i < n; i++)
				{
					sumFast += abs[i];
					if (i >= fastWin)
					{
						sumFast -= abs[i - fastWin];
					}

					fast[i] = sumFast / Math.Min(i + 1, fastWin);

					sumSlow += abs[i];
					if (i >= slowWin)
					{
						sumSlow -= abs[i - slowWin];
					}

					slow[i] = sumSlow / Math.Min(i + 1, slowWin);
				}

				double[] novelty = new double[n];
				for (int i = 0; i < n; i++)
				{
					novelty[i] = Math.Max(0.0, fast[i] - slow[i]);
				}

				// DC entfernen und auf Varianz normieren
				double mean = novelty.Average();
				double var = 0.0;
				for (int i = 0; i < n; i++)
				{
					novelty[i] -= mean;
					var += novelty[i] * novelty[i];
				}
				if (var <= 1e-12)
				{
					return 0.0;
				}

				double invStd = 1.0 / Math.Sqrt(var / n);
				for (int i = 0; i < n; i++)
				{
					novelty[i] *= invStd;
				}

				// 2) Autokorrelation via FFT: r = IFFT(|FFT(x)|^2)
				int L = 1;
				while (L < 2 * n)
				{
					L <<= 1;
				}

				var fft = new MathNet.Numerics.Complex32[L];
				for (int i = 0; i < n; i++)
				{
					fft[i] = new MathNet.Numerics.Complex32((float) novelty[i], 0f);
				}

				for (int i = n; i < L; i++)
				{
					fft[i] = MathNet.Numerics.Complex32.Zero;
				}

				Fourier.Forward(fft, FourierOptions.Matlab);

				for (int i = 0; i < L; i++)
				{
					// |X|^2 = X * conj(X)
					var v = fft[i];
					fft[i] = new MathNet.Numerics.Complex32(v.Magnitude * v.Magnitude, 0f);
				}

				Fourier.Inverse(fft, FourierOptions.Matlab);

				// Realteil, 0..n-1 relevant (lineare Autokorrelation)
				double r0 = Math.Max(fft[0].Real, 1e-12f);
				// 3) Lag-Suchbereich aus BPM-Grenzen
				int minLag = (int) Math.Round(sampleRate * 60.0 / Math.Max(maxBpm, 1));
				int maxLag = (int) Math.Round(sampleRate * 60.0 / Math.Max(minBpm, 1));
				minLag = Math.Clamp(minLag, 1, n - 1);
				maxLag = Math.Clamp(maxLag, minLag, n - 1);

				// 4) Bestes Lag im Bereich wählen (größte normalisierte Autokorrelation)
				int bestLag = -1;
				double bestVal = double.NegativeInfinity;

				for (int k = minLag; k <= maxLag; k++)
				{
					double val = fft[k].Real / r0; // Normalisierung
					if (val > bestVal)
					{
						bestVal = val;
						bestLag = k;
					}
				}

				if (bestLag <= 0 || bestVal < 0.02) // zu schwaches Signal
				{
					return 0.0;
				}

				// 5) Parabolische Interpolation um Maximum (Sub-Sample-Schätzung)
				double lag = bestLag;
				if (bestLag > minLag && bestLag < maxLag)
				{
					double y1 = fft[bestLag - 1].Real;
					double y2 = fft[bestLag].Real;
					double y3 = fft[bestLag + 1].Real;
					double denom = (y1 - 2 * y2 + y3);
					if (Math.Abs(denom) > 1e-12)
					{
						double delta = 0.5 * (y1 - y3) / denom; // in [-1,1]
						delta = Math.Max(-1.0, Math.Min(1.0, delta));
						lag = bestLag + delta;
					}
				}

				// 6) BPM berechnen
				double bpm = 60.0 * sampleRate / lag;

				// Optional: auf Bereich [minBpm,maxBpm] falten (x2 / x0.5 Heuristik)
				while (bpm < minBpm && bpm > 0)
				{
					bpm *= 2.0;
				}

				while (bpm > maxBpm)
				{
					bpm /= 2.0;
				}

				return (bpm >= minBpm && bpm <= maxBpm) ? bpm : 0.0;
			});
		}
	}
}
