#pragma once

#ifdef __cplusplus
extern "C" {
#endif

// Export macro
#ifndef PIXELIZE_API
#ifdef _WIN32
    #define PIXELIZE_API __declspec(dllexport)
#else
    #define PIXELIZE_API __attribute__((visibility("default")))
#endif
#endif

#include <cstdint>


typedef struct ass_image {
    int w, h;
    int stride;
    unsigned char* bitmap;
    uint32_t color;
    int dst_x, dst_y;
    struct ass_image* next;
    enum {
        IMAGE_TYPE_CHARACTER,
        IMAGE_TYPE_OUTLINE,
        IMAGE_TYPE_SHADOW
    } type;
} ASS_Image;

PIXELIZE_API void render_subs(unsigned char* frameData, int width, int height, ASS_Image* img);

#ifdef __cplusplus
}
#endif
