#include "pixelize.hpp"

PIXELIZE_API void render_subs(unsigned char* frameData, unsigned char* frameCopy, int width, int height, ASS_Image* img) {
    std::copy(frameData, frameData + (width * height * 4), frameCopy); // Copy original frame data

    for (; img; img = img->next) {
        unsigned int o = 255 - img->color & 0xFF;
        unsigned int r = img->color >> 24;
        unsigned int g = (img->color >> 16) & 0xFF;
        unsigned int b = (img->color >> 8) & 0xFF;
        unsigned int a = img->color & 0xFF;

        for (int y = 0; y < img->h; y++) {
            for (int x = 0; x < img->w; x++) {
                // Source pixel index
                int srcIndex = y * img->stride + x;

                // Destination pixel index
                int destX = x + img->dst_x;
                int destY = y + img->dst_y;

                if (destX >= 0 && destX < width && destY >= 0 && destY < height) {
                    int destIndex = (destY * width + destX) * 4;

                    unsigned char srcAlpha = img->bitmap[srcIndex];
                    unsigned int k = ((unsigned)srcAlpha) * o / 255;
                    unsigned int ck = 255 - k;

                    frameData[destIndex + 0] = (k * b + ck * frameData[destIndex + 0]) / 255;
                    frameData[destIndex + 1] = (k * g + ck * frameData[destIndex + 1]) / 255;
                    frameData[destIndex + 2] = (k * r + ck * frameData[destIndex + 2]) / 255;
                    frameData[destIndex + 3] = 255; // Opaque
                }
            }
        } 
    }
}