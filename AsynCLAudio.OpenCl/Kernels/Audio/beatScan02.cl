#pragma OPENCL EXTENSION cl_khr_fp64 : enable

typedef struct {
    float x; // real part
    float y; // imaginary part
} Vector2;

#ifndef M_PI
#define M_PI 3.14159265358979323846f
#endif

__kernel void beatScan01(
    __global const Vector2* input,
    __global double* output,
    const long length,
    const int samplerate,
    const int bitDepth
) {
    int gid = get_global_id(0);

    // Only process valid indices
    if (gid >= length) return;

    // 1. Compute power spectrum (|FFT|^2)
    double power = (double)(input[gid].x * input[gid].x + input[gid].y * input[gid].y);

    // 2. Bandpass filter for typical BPM ranges (40-220 BPM)
    double freq = (double)gid * samplerate / length;
    double minBPM = 60.0;
    double maxBPM = 240.0;
    double minFreq = minBPM / 60.0;
    double maxFreq = maxBPM / 60.0;

    // Apply frequency filter
    if (freq < minFreq || freq > maxFreq) {
        power = 0.0;
    }
    
    // 3. First pass: store power spectrum for BPM calculation
    output[gid] = power;

    // Barrier to ensure all work items have written their results
    barrier(CLK_GLOBAL_MEM_FENCE);

    // 4. Determine the dominant BPM
    // This part runs only on a single work item (gid 0) to avoid race conditions.
    if (gid == 0) {
        double maxPower = 0.0;
        int peakIndex = 0;
        
        // Search for peak in the valid BPM frequency range
        int minIndex = (int)ceil((length * minFreq) / (double)samplerate);
        int maxIndex = (int)floor((length * maxFreq) / (double)samplerate);
        
        for (int i = minIndex; i <= maxIndex && i < length; i++) {
            if (output[i] > maxPower) {
                maxPower = output[i];
                peakIndex = i;
            }
        }
        
        double dominantFreq = 0.0;
        if (peakIndex > 0) {
            dominantFreq = (double)peakIndex * samplerate / length;
            double dominantBPM = dominantFreq * 60.0;
            
            // Store result in the first output element
            output[0] = dominantBPM;
        } else {
            output[0] = 0.0;
        }

        // Clear other outputs (optional)
        for (int i = 1; i < length; i++) {
            output[i] = 0.0;
        }
    }
}