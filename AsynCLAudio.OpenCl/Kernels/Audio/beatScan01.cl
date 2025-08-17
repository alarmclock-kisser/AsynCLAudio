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
    double minBPM = 40.0;
    double maxBPM = 220.0;
    double minFreq = minBPM / 60.0;
    double maxFreq = maxBPM / 60.0;
    
    // Apply frequency filter
    if (freq < minFreq || freq > maxFreq) {
        power = 0.0;
    }
    
    // 3. First pass: store power spectrum for autocorrelation
    output[gid] = power;
    
    // Barrier to ensure all work items have written their results
    barrier(CLK_GLOBAL_MEM_FENCE);

    // 4. Autocorrelation via inverse FFT of power spectrum
    if (gid == 0) {
        // This part would normally require an IFFT, but we're simplifying
        // by just finding the peak in the power spectrum
        
        double maxPower = 0.0;
        int peakIndex = 0;
        
        // Search for peak in the valid BPM range
        int minIndex = (int)(length * minFreq / samplerate);
        int maxIndex = (int)(length * maxFreq / samplerate);
        
        for (int i = minIndex; i < maxIndex && i < length; i++) {
            if (output[i] > maxPower) {
                maxPower = output[i];
                peakIndex = i;
            }
        }
        
        // Convert peak index to BPM
        if (peakIndex > 0) {
            double dominantBPM = 60.0 * samplerate / peakIndex;
            
            // Store result in first output element
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