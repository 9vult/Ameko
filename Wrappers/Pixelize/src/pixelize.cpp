#include "pixelize.hpp"

unsigned char blend_alpha(unsigned char srcAlpha, unsigned char dstAlpha) {
    float src = srcAlpha / 255.0f;
    float dst = dstAlpha / 255.0f;
    float blend = 1.0f - (1.0f - src) * (1.0f - dst);
    return (unsigned char)(blend * 255);
}

PIXELIZE_API void render_subs(unsigned char* frameData, int width, int height, ASS_Image* img) {
    for (; img; img = img->next) {
        for (int y = 0; y < img->h; y++) {
            for (int x = 0; x < img->w; x++) {
                // Source pixel index
                int srcIndex = y * img->stride + x;

                // Destination pixel index
                int destX = x + img->dst_x;
                int destY = y + img->dst_y;

                if (destX >= 0 && destX < width && destY >= 0 && destY < height) {
                    int destIndex = (destY * width + destX);

                    // Get the alpha values of the current image and of the frame
                    unsigned char existingAlpha = frameData[destIndex];
                    unsigned char newAlpha = img->bitmap[srcIndex];

                    // Calculate the blended alpha
                    unsigned char blendedAlpha = blend_alpha(newAlpha, existingAlpha);

                    // Write to the frame
                    frameData[destIndex] = blendedAlpha;
                }
            }
        } 
    }
}