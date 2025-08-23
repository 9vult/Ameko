// SPDX-License-Identifier: MPL-2.0

//! Implementation of the Fowler–Noll–Vo 1a hash function

const prime: u32 = 0x01000193;
const offset: u32 = 0x811c9dc5;

pub fn fnv1a_32(data: [*c]const u8, data_len: c_int) u32 {
    var hash: u32 = offset;

    var i: usize = 0;
    while (i < data_len - 1) : (i += 1) {
        const byte = data[i];
        hash ^= byte;
        hash *%= prime;
    }
    return hash;
}
