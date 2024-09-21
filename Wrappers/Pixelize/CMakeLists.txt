cmake_minimum_required(VERSION 3.10)
project(Pixelize VERSION 1.0 LANGUAGES CXX)

# Define library output type
add_library(Pixelize SHARED
    src/pixelize.cpp
)

# Add include directories
target_include_directories(Pixelize PUBLIC include)

# Cross-platform dynamic library properties
if(WIN32)
    set_target_properties(Pixelize PROPERTIES
        PREFIX ""
        SUFFIX ".dll")
elseif(APPLE)
    set_target_properties(Pixelize PROPERTIES
        PREFIX "" # lib?
        SUFFIX ".dylib")
else()
    set_target_properties(Pixelize PROPERTIES
        PREFIX "" # lib?
        SUFFIX ".so")
endif()

# Set the C++ standard
set_target_properties(Pixelize PROPERTIES CXX_STANDARD 11 CXX_STANDARD_REQUIRED YES)

# Installation (optional)
install(TARGETS Pixelize DESTINATION lib)
install(FILES include/pixelize.h DESTINATION include)
