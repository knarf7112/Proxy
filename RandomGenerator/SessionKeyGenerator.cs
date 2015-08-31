﻿using System;
using System.IO;
using System.Text;
//
using System.Security.Cryptography;

namespace RandomGenerator
{
    /// <summary>
    /// 產生指定的RanDom A 值
    /// </summary>
    public class SessionKeyGenerator : ISessionKeyGenerator
    {
        #region Field
        /// <summary>
        /// 指定的長度
        /// </summary>
        public static readonly int DataLength = 16;

        //選擇用的 Random A(4096個byte)
        private static readonly byte[] _randData = new byte[]
        {
            0x47, 0xB3, 0x36, 0xB9, 0x46, 0xE3, 0x15, 0xCC, 0x58, 0x2F, 0xAA, 0x8E, 0xEB, 0x6A, 0x5E, 0x1E, 0xE9, 0xB3, 0x14, 0x71, 0xC2, 0xE9, 0x96, 0xC3, 0x92, 0xF9, 0x43, 0xA3, 0xFA, 0xC1, 0xEF, 0x49, 0xAA, 0x8C, 0x63, 0xD1, 0x56, 0x3A, 0xBC, 0x67, 0x87, 0xB1, 0x33, 0xC4, 0x5B, 0x5E, 0x7F, 0x3F, 0x25, 0x18, 0x20, 0xB9, 0xB7, 0x71, 0x9E, 0x79, 0xD9, 0xE4, 0xCA, 0xAA, 0x56, 0x35, 0xD1, 0x62, 0xAA, 0xD2, 0x26, 0xEE, 0x6E, 0xCE, 0x1D, 0x36, 0xFB, 0xDE, 0x01, 0x62, 0xB3, 0x6D, 0x02, 0xB9, 0x93, 0x04, 0x39, 0x00, 0x5D, 0x51, 0x54, 0x95, 0x82, 0x55, 0x48, 0x8B, 0xAD, 0x7D, 0x9F, 0x64, 0x36, 0x19, 0xF0, 0x58, 0xD7, 0x71, 0x6B, 0x7E, 0x16, 0x35, 0x2C, 0xAF, 0x9D, 0x06, 0xBB, 0x1A, 0x95, 0x53, 0xCD, 0x98, 0x78, 0x7B, 0xD8, 0xF5, 0x27, 0xBF, 0x9E, 0xD8, 0x95, 0x8A, 0x9B, 0xF6, 0x78, 0x23, 0x22, 0x23, 0x4E, 0xE4, 0xBB, 0x1D, 0x68, 0x45, 0xC6, 0x70, 0x4A, 0x62, 0xAB, 0x69, 0xF4, 0x7B, 0xF4, 0xB0, 0x7A, 0xB3, 0x25, 0xC8, 0xD1, 0xC8, 0xFE, 0x9C, 0x51, 0x22, 0xC0, 0x4C, 0x75, 0xCB, 0x6B, 0x03, 0x4F, 0xE7, 0xDC, 0x95, 0xCD, 0x52, 0x16, 0x26, 0xD3, 0x87, 0xA6, 0x28, 0xC0, 0x9A, 0x58, 0xCC, 0xFA, 0x8E, 0xD6, 0x2B, 0xC8, 0xF4, 0x67, 0x03, 0x52, 0x2D, 0x69, 0x60, 0xE9, 0x38, 0xC8, 0xD5, 0x61, 0xA3, 0xF1, 0xC9, 0xF1, 0xAF, 0xF2, 0xD5, 0xB8, 0x8F, 0xEB, 0xBA, 0x15, 0x36, 0x49, 0x77, 0x4F, 0x98, 0xD9, 0x3E, 0x2E, 0xCF, 0x15, 0x85, 0x65, 0xCD, 0xA1, 0x1B, 0x00, 0x95, 0x9E, 0x88, 0xC5, 0x8A, 0xBF, 0x01, 0xFC, 0x69, 0xF5, 0xE6, 0x89, 0xCC, 0x4E, 0x69, 0x96, 0xE7, 0x00, 0x35, 0x63, 0x30, 0x12, 0x11, 0xF6, 0xD8, 0x48, 0x04, 0x62, 0x68, 0x59, 0x08, 0xD0, 0xB7, 0x93, 0x51, 0x92, 0x20, 0xC4, 0xE2, 0x14, 0x46, 0x0C, 0x84, 0x20, 0xB4, 0x47, 0x10, 0x12, 0x85, 0x6E, 0x76, 0x0C, 0xD8, 0xBC, 0x5C, 0xEF, 0x21, 0xC6, 0xEF, 0xC5, 0xC9, 0xA5, 0x95, 0xBC, 0x1B, 0xA3, 0xEB, 0x4A, 0x66, 0xD2, 0x17, 0x3F, 0x49, 0x63, 0xCF, 0x5A, 0x56, 0x36, 0x7A, 0xF4, 0x1A, 0xEE, 0x52, 0xB2, 0x9E, 0xAF, 0xDF, 0x56, 0x35, 0x61, 0x67, 0xB7, 0x55, 0x7E, 0xBE, 0x30, 0xAA, 0xBA, 0x45, 0x3C, 0xC0, 0x3E, 0xB1, 0x23, 0xE6, 0xBB, 0x82, 0xA8, 0x99, 0x8F, 0x84, 0x5D, 0x17, 0xBD, 0x44, 0x80, 0xE7, 0xCC, 0xF8, 0x27, 0x69, 0x70, 0xC4, 0x53, 0x71, 0x7C, 0xFD, 0xF7, 0x62, 0x56, 0x96, 0x8A, 0x77, 0x51, 0x58, 0x07, 0x7D, 0xF3, 0xEF, 0x62, 0x3A, 0x3C, 0x96, 0xC5, 0xDF, 0xA2, 0xD8, 0x33, 0x48, 0x9C, 0x59, 0x7A, 0x62, 0xB5, 0x9F, 0x80, 0x2D, 0xBB, 0xB6, 0x46, 0x4D, 0x18, 0x40, 0xE4, 0xCD, 0x5A, 0x8D, 0xE0, 0x5A, 0x8B, 0x7E, 0x41, 0xFD, 0x4F, 0xBA, 0x4B, 0x2B, 0x30, 0x3B, 0xDE, 0x93, 0xF5, 0x9E, 0x95, 0xD1, 0x2D, 0x56, 0xB4, 0x68, 0x05, 0x96, 0xD8, 0x57, 0x48, 0x7C, 0xF5, 0xC4, 0xC2, 0x84, 0xA6, 0xD0, 0xF7, 0x6F, 0x77, 0xC0, 0x59, 0x2C, 0x55, 0x5D, 0xB6, 0xBC, 0xC9, 0x9D, 0x81, 0xFB, 0x99, 0xDF, 0x12, 0x2F, 0x72, 0xB0, 0xB4, 0xB1, 0xC0, 0xBB, 0xB2, 0xFD, 0x70, 0x9A, 0x01, 0xB6, 0x11, 0xB2, 0xBC, 0x4D, 0xFB, 0xA5, 0x8B, 0xC3, 0xA4, 0x2B, 0xF9, 0xF2, 0xB6, 0xEB, 0xB8, 0x6D, 0xD6, 0x75, 0x6F, 0x30, 0x22, 0x70, 0x3B, 0x6E, 0xEA, 0x5D, 0xEC, 0xF0, 0x1B, 0xB3, 0xE6, 0x5B, 0x24, 0x1C, 0xAB, 0x62, 0xA3, 0x4D, 0x67, 0x82, 0xAB, 0xEF, 0x1A, 0x12, 0x02, 0xD0, 0xA9, 0x9E, 0x09, 0x9D, 0x5B, 0x7A, 0xF4, 0x4A, 0x78, 0x22, 0xB7, 0x0E, 0x32, 0x66, 0x3B, 0xAE, 0x5A, 0xBC, 0xD0, 0x3E, 0xD7, 0x1B, 0x3B, 0x4D, 0xA7, 0x06, 0x7B, 0x83, 0x38, 0xE1, 0x15, 0xD7, 0x39, 0xA2, 0xF4, 0x6F, 0x85, 0x13, 0x5B, 0x76, 0x50, 0x54, 0xB5, 0x56, 0x2A, 0x24, 0x04, 0xD1, 0xC7, 0x77, 0x5F, 0x22, 0x78, 0xFD, 0x61, 0x85, 0x28, 0x65, 0x15, 0x0B, 0x85, 0xFD, 0x2A, 0xC8, 0x28, 0xC6, 0x0E, 0x17, 0xF8, 0x71, 0x20, 0xC6, 0x52, 0x56, 0xBB, 0xBE, 0xEE, 0x5F, 0x5C, 0xD1, 0x2C, 0x6F, 0xAD, 0x03, 0x8C, 0x0E, 0xCB, 0x19, 0x92, 0xB9, 0xC1, 0x15, 0x0C, 0x75, 0xA2, 0xCE, 0x2B, 0xCA, 0xFE, 0x09, 0xF1, 0xE8, 0xDD, 0x1F, 0x4E, 0xEE, 0xFB, 0xED, 0x39, 0xB3, 0x33, 0x0B, 0x8A, 0xCF, 0x8B, 0x31, 0x61, 0xF6, 0xB3, 0xA4, 0x20, 0xF6, 0x8B, 0x93, 0x0A, 0xEE, 0xC1, 0x79, 0xE2, 0x2C, 0x37, 0x71, 0xBE, 0xC0, 0xBF, 0x20, 0xE9, 0xFB, 0x5B, 0x75, 0xAC, 0x8F, 0x93, 0x02, 0x1D, 0x64, 0x27, 0xFE, 0xCD, 0x45, 0xBF, 0xE6, 0xCF, 0xC6, 0x3C, 0x3C, 0xC6, 0x5C, 0x0D, 0x98, 0x77, 0x24, 0x5B, 0x7E, 0xF3, 0xC3, 0x9A, 0xAF, 0xF6, 0x2C, 0x31, 0xE4, 0xAC, 0xB6, 0x68, 0x2F, 0x48, 0x69, 0x03, 0x10, 0xB3, 0x61, 0xC5, 0x6C, 0x16, 0xE0, 0xBA, 0x25, 0xFC, 0xC8, 0xC4, 0xE2, 0x47, 0xE1, 0xB3, 0xD0, 0xBC, 0x03, 0x43, 0x84, 0x52, 0xFC, 0x8F, 0x6D, 0x03, 0xFD, 0x0C, 0x70, 0x5D, 0xAF, 0xBA, 0x24, 0x87, 0x6E, 0xE5, 0x0D, 0x3D, 0x43, 0x4B, 0xF9, 0x95, 0x2B, 0x3E, 0x5C, 0xF4, 0x8F, 0x17, 0x2B, 0x8E, 0xC6, 0x86, 0x14, 0x4A, 0xAB, 0xF9, 0x72, 0xD9, 0xDF, 0x89, 0x89, 0x1B, 0x6B, 0xAF, 0x62, 0x7D, 0x4F, 0x33, 0x45, 0xBB, 0x54, 0xF3, 0x87, 0x17, 0x98, 0x3A, 0xAE, 0x7A, 0xDA, 0x1D, 0x88, 0x3C, 0x32, 0xDD, 0x2E, 0x97, 0xE3, 0x84, 0x82, 0x20, 0x57, 0xD3, 0x9D, 0x22, 0xDF, 0xFF, 0x53, 0x37, 0x5E, 0xAD, 0x73, 0xB5, 0x28, 0x55, 0x04, 0xA4, 0x91, 0x0E, 0x66, 0x1D, 0x43, 0x15, 0xB3, 0xAE, 0x45, 0x1F, 0xE5, 0x90, 0xF0, 0x2B, 0xB3, 0x07, 0xC9, 0xDE, 0xC7, 0xDF, 0xFF, 0x2C, 0xD5, 0xC7, 0xB0, 0x91, 0xF5, 0xB1, 0x5A, 0x81, 0x13, 0xD4, 0x7E, 0xD7, 0x63, 0xCC, 0x15, 0xA5, 0xD8, 0x82, 0x24, 0x46, 0x4D, 0x0E, 0x3F, 0xB3, 0xF1, 0xA7, 0x82, 0xE0, 0x60, 0x91, 0xC5, 0xF0, 0xE6, 0x77, 0x68, 0x0D, 0x4E, 0xFC, 0x0E, 0xC3, 0xC5, 0x27, 0x43, 0xC9, 0xB3, 0x2E, 0xCE, 0xAC, 0xBF, 0xFA, 0x47, 0x6F, 0xB7, 0xEC, 0x85, 0x5B, 0x9C, 0x03, 0xC3, 0xBC, 0x4E, 0xCC, 0xAC, 0x5C, 0xEE, 0xE4, 0x22, 0x1E, 0x2B, 0xAE, 0xB5, 0xA5, 0x05, 0x46, 0xCB, 0xCE, 0x5A, 0x17, 0x20, 0x98, 0xBB, 0x89, 0x36, 0xD4, 0x64, 0x3C, 0xCD, 0xF5, 0xFF, 0x07, 0xF2, 0x04, 0xE3, 0x25, 0xD5, 0xAF, 0x8C, 0x1D, 0xBB, 0x47, 0x2A, 0xA4, 0x07, 0x11, 0x58, 0x55, 0x04, 0xC1, 0x25, 0x4B, 0xB1, 0x63, 0x23, 0xE7, 0x2E, 0x70, 0x00, 0xC9, 0x39, 0x54, 0xC6, 0x00, 0x7B, 0x0B, 0x78, 0xAB, 0xF4, 0x2F, 0x3B, 0x22, 0x2E, 0xE8, 0x1C, 0x23, 0x09, 0x8F, 0xD6, 0x25, 0xCA, 0x71, 0x88, 0x54, 0xA5, 0x83, 0xB0, 0xB8, 0xFC, 0x9E, 0x96, 0x5D, 0x3B, 0x4B, 0xD2, 0x58, 0xA5, 0xD6, 0x0F, 0xFB, 0x32, 0x9D, 0x5A, 0x9C, 0x71, 0x4C, 0x7C, 0xDE, 0x6E, 0xB2, 0x18, 0x76, 0xF4, 0x79, 0x6F, 0xF6, 0x64, 0x4D, 0x94, 0xA4, 0xD2, 0xE3, 0x0D, 0xD8, 0x83, 0x40, 0xC2, 0xC0, 0xDA, 0x61, 0x38, 0xB9, 0x85, 0xEF, 0x49, 0xFC, 0x2C, 0xA9, 0x9B, 0xDD, 0x83, 0x9C, 0x15, 0xE0, 0xBB, 0xE9, 0x02, 0x1A, 0x34, 0x1C, 0x61, 0x41, 0x7A, 0x6D, 0x66, 0x34, 0xDE, 0x1F, 0xA8, 0x65, 0x11, 0x33, 0x90, 0x72, 0xFD, 0xED, 0x63, 0xD6, 0x4A, 0x12, 0x84, 0x31, 0xEA, 0x5C, 0x6F, 0x0C, 0x63, 0xCC, 0x6A, 0x18, 0x88, 0x67, 0xB4, 0x72, 0x54, 0x06, 0x03, 0xBF, 0x20, 0xF9, 0x39, 0x88, 0xB9, 0x24, 0x94, 0xE6, 0x37, 0x7A, 0x66, 0x47, 0xF7, 0x75, 0x2A, 0xF5, 0xEE, 0x81, 0xD8, 0xBC, 0xAC, 0xF0, 0x4C, 0x7A, 0xAB, 0xD8, 0xDA, 0xE3, 0x9B, 0xF9, 0xF7, 0x60, 0xAC, 0x1E, 0xFE, 0x81, 0xBB, 0x59, 0x4B, 0x70, 0x4B, 0x19, 0xE0, 0xB5, 0xEF, 0xAB, 0xF8, 0x4D, 0xA4, 0x48, 0x62, 0x14, 0xB0, 0x5E, 0xA8, 0xE1, 0xE5, 0x9D, 0x29, 0xEE, 0x59, 0x11, 0xE1, 0x6C, 0x4E, 0xAD, 0x78, 0x00, 0xD5, 0xC6, 0x75, 0x44, 0xCD, 0xDF, 0xA6, 0x7C, 0x8C, 0xB7, 0xD4, 0xB7, 0xDF, 0x07, 0xA1, 0x97, 0x24, 0xCC, 0x3A, 0x99, 0x0C, 0x18, 0x00, 0xF4, 0x0D, 0xC7, 0xC6, 0xF6, 0x35, 0x34, 0x43, 0xEE, 0x64, 0x37, 0xF2, 0xB8, 0x49, 0x4A, 0xBB, 0x55, 0x11, 0xFB, 0x87, 0x3F, 0x93, 0x99, 0x62, 0xAC, 0xE9, 0x9C, 0xBB, 0x3A, 0x72, 0x0E, 0x2D, 0x9D, 0x35, 0x4A, 0x97, 0xB0, 0x0E, 0x57, 0x7D, 0xDA, 0xDD, 0xB5, 0x4C, 0xE7, 0x97, 0x11, 0x3E, 0x94, 0xC3, 0xE1, 0x66, 0xBA, 0x9B, 0x83, 0xBD, 0x04, 0xBA, 0xB8, 0x1F, 0xD2, 0xD1, 0x64, 0x83, 0xA1, 0x18, 0x60, 0xDA, 0xFE, 0x1D, 0xE9, 0x48, 0x6C, 0xFB, 0xEF, 0x27, 0xAC, 0xDC, 0x00, 0xA6, 0xF4, 0x3A, 0x16, 0x83, 0x6C, 0x34, 0x14, 0xDF, 0xE9, 0x10, 0x52, 0xFB, 0x66, 0x6A, 0xCD, 0x19, 0x76, 0xA5, 0xE4, 0xDE, 0xF5, 0xE1, 0x41, 0x1E, 0x6A, 0xBB, 0xCC, 0x9A, 0x24, 0x05, 0x8F, 0x6F, 0xBF, 0x7D, 0x09, 0x98, 0x26, 0x9F, 0x5D, 0xC9, 0x18, 0xF0, 0x3C, 0x81, 0xF5, 0x0F, 0x7C, 0xB9, 0x76, 0x9D, 0x32, 0xDF, 0xE1, 0x3A, 0x5C, 0x5D, 0x04, 0x27, 0x08, 0x01, 0xE3, 0x0E, 0x64, 0x95, 0x94, 0x05, 0x0B, 0x99, 0x75, 0x8C, 0x4B, 0xD9, 0x56, 0x07, 0x5F, 0x72, 0x75, 0xB2, 0x40, 0x84, 0x9A, 0x91, 0x55, 0xCC, 0xA8, 0x84, 0x63, 0x4B, 0x44, 0x56, 0xEB, 0xA7, 0xE2, 0xD9, 0xA7, 0xC5, 0x8B, 0xCD, 0x7B, 0xAB, 0xB9, 0xCC, 0xF7, 0x08, 0xBF, 0x2B, 0x55, 0x50, 0x36, 0x77, 0xD7, 0x1C, 0x54, 0xA6, 0xD9, 0xFA, 0x8C, 0xF0, 0x97, 0xA2, 0x9C, 0x8F, 0xC0, 0x81, 0x7B, 0xED, 0x38, 0x10, 0xE2, 0x17, 0xFD, 0xFC, 0x02, 0xB3, 0xFF, 0xED, 0x3F, 0x88, 0x59, 0x18, 0xF0, 0x5E, 0xA6, 0xD3, 0xB2, 0x93, 0x41, 0x2F, 0x79, 0xAE, 0x68, 0x57, 0x55, 0x77, 0xBD, 0x63, 0x46, 0x18, 0xCC, 0x83, 0xE2, 0x00, 0xED, 0x17, 0x04, 0xDC, 0x99, 0x0E, 0x3E, 0x81, 0x32, 0x6A, 0xF9, 0x69, 0x77, 0x61, 0x8E, 0xA5, 0x4C, 0x82, 0xC4, 0x08, 0x44, 0x16, 0x36, 0x8D, 0x60, 0x86, 0x3E, 0x0B, 0x1E, 0x07, 0x3B, 0x4F, 0x89, 0xCF, 0xB9, 0x35, 0xC8, 0x86, 0x93, 0x0E, 0x42, 0x4D, 0x9C, 0x78, 0x72, 0x71, 0xF7, 0xFA, 0xA5, 0xAD, 0x97, 0xB1, 0x2C, 0x27, 0xB1, 0x5E, 0xDA, 0x60, 0x5B, 0x30, 0xAB, 0x56, 0xF5, 0x46, 0x70, 0x2F, 0x1E, 0x3B, 0x9C, 0x2A, 0xA0, 0x7C, 0xDC, 0x4B, 0x3E, 0x0F, 0x57, 0x21, 0x35, 0xE6, 0xDE, 0xF1, 0x10, 0x5D, 0xA9, 0x8A, 0x7B, 0x33, 0x0E, 0x57, 0x61, 0xF7, 0x41, 0x83, 0xFB, 0x23, 0xEF, 0xA7, 0x34, 0x8B, 0x01, 0x49, 0xD6, 0x9B, 0x08, 0xBF, 0x1B, 0x9E, 0xAA, 0x32, 0xB7, 0x7B, 0x0B, 0x50, 0x2E, 0xE5, 0x12, 0x4F, 0x15, 0xD8, 0x57, 0x71, 0x66, 0xE8, 0xD4, 0x4D, 0x36, 0x9D, 0x3C, 0xFE, 0x64, 0xC7, 0x80, 0xDA, 0x68, 0x41, 0xC6, 0xF6, 0x89, 0x62, 0x82, 0x82, 0xF1, 0x1D, 0xC3, 0x38, 0x7C, 0xB7, 0xBB, 0x54, 0xDF, 0x96, 0xCA, 0x18, 0x23, 0x66, 0x0C, 0xB8, 0x6A, 0x4D, 0xD6, 0x67, 0x2B, 0xA5, 0xDB, 0xAA, 0xD5, 0xC2, 0x6D, 0xB9, 0xC4, 0x75, 0xDD, 0x70, 0xD8, 0xA2, 0x84, 0xFF, 0xA6, 0x61, 0xE1, 0x80, 0xE7, 0x9C, 0x3F, 0x10, 0x77, 0x1D, 0x06, 0xEE, 0x08, 0xB9, 0x94, 0xD9, 0x39, 0xD9, 0x63, 0xFC, 0xA2, 0x03, 0xC2, 0x61, 0x8E, 0x82, 0x16, 0xB3, 0x05, 0x98, 0x25, 0xA1, 0x25, 0x7B, 0x72, 0xC3, 0x99, 0xC3, 0x68, 0x74, 0x04, 0xC3, 0x0C, 0xE9, 0x78, 0x8E, 0xA2, 0x04, 0x14, 0x6F, 0xA9, 0xD9, 0x2F, 0x89, 0x25, 0x32, 0x51, 0x22, 0x77, 0xA9, 0x96, 0x6D, 0x53, 0x5D, 0x02, 0xB1, 0xDF, 0xB9, 0xCA, 0xF5, 0x5E, 0x3E, 0x25, 0x4E, 0x14, 0x81, 0x38, 0x14, 0x6E, 0x91, 0x32, 0xF6, 0x34, 0x79, 0xBF, 0xF4, 0xF7, 0x14, 0xAB, 0xC4, 0x2C, 0x2E, 0xE3, 0xCD, 0x6A, 0x63, 0x32, 0x6E, 0x39, 0x80, 0x74, 0x07, 0x85, 0x23, 0xAB, 0xBE, 0x91, 0xFF, 0x43, 0x9F, 0x66, 0x50, 0x5A, 0x16, 0x80, 0x9C, 0x19, 0xD6, 0xF4, 0xB4, 0x93, 0x6F, 0xA4, 0xC7, 0xF0, 0x23, 0xF4, 0xB0, 0x20, 0x79, 0xFB, 0x08, 0x17, 0xDF, 0x8A, 0x66, 0xF3, 0x7E, 0x2D, 0x19, 0x08, 0x7E, 0x55, 0x97, 0x2C, 0xA7, 0x61, 0x36, 0xC6, 0x41, 0xD0, 0x5D, 0xB2, 0x6A, 0x7C, 0x07, 0xBC, 0x5B, 0x1F, 0xD6, 0xFE, 0x52, 0x6D, 0xD0, 0xFF, 0xBA, 0x6A, 0xA8, 0x37, 0x37, 0xFE, 0xCA, 0xE3, 0x8A, 0x43, 0x00, 0x34, 0x8D, 0x43, 0x2A, 0x70, 0xAC, 0x42, 0x80, 0x20, 0x46, 0x47, 0x20, 0x24, 0x49, 0xBE, 0xB7, 0xBC, 0x17, 0x09, 0x3D, 0x1E, 0x2B, 0x09, 0xDF, 0x75, 0x89, 0x79, 0x73, 0x9E, 0x74, 0xA1, 0xE5, 0x89, 0x1E, 0x12, 0x5A, 0x45, 0x22, 0x9A, 0x24, 0xCE, 0x7D, 0x74, 0x89, 0x53, 0xD8, 0x27, 0x30, 0xE9, 0xB5, 0xDB, 0x7C, 0xA7, 0x53, 0x69, 0xE6, 0xFD, 0x5D, 0xEB, 0xAB, 0x71, 0xBB, 0x8B, 0x16, 0x07, 0x95, 0x40, 0x9C, 0x33, 0x6C, 0x05, 0x71, 0x4D, 0xB9, 0x1C, 0xFE, 0xCA, 0x8B, 0xFF, 0x4B, 0x46, 0x8F, 0x09, 0x84, 0xCD, 0x31, 0x05, 0x66, 0x90, 0x47, 0x6F, 0xF0, 0xEC, 0xDB, 0x33, 0xF8, 0x7F, 0x1A, 0xAD, 0x42, 0x78, 0x75, 0x4F, 0x2B, 0x18, 0x9B, 0x22, 0x28, 0x0F, 0xD3, 0xEF, 0xBF, 0xE0, 0x35, 0x6B, 0x0A, 0xB7, 0x46, 0x4F, 0xC2, 0xE2, 0x74, 0x47, 0x0C, 0xCA, 0x0E, 0x3C, 0x65, 0xEF, 0x77, 0xF3, 0x92, 0xE7, 0x58, 0xFE, 0xEB, 0xD6, 0x62, 0xA4, 0x9E, 0xE9, 0xDB, 0xB5, 0x13, 0x1F, 0x46, 0x6D, 0x7F, 0xFF, 0xB9, 0x56, 0xD4, 0x65, 0xF7, 0xF2, 0xFF, 0x5F, 0x26, 0x24, 0xA1, 0x13, 0xF7, 0x68, 0xEE, 0xBF, 0x60, 0xEF, 0xCC, 0x9E, 0x65, 0x97, 0x29, 0xE6, 0xFF, 0x87, 0x0B, 0x57, 0xF8, 0x9E, 0xFE, 0x89, 0x17, 0x95, 0x5B, 0x25, 0x5F, 0xB5, 0xAD, 0x0A, 0xA5, 0x54, 0xE3, 0x6B, 0xA5, 0x62, 0x84, 0x21, 0x8E, 0x90, 0x0B, 0x5E, 0x4F, 0x55, 0x91, 0x3A, 0x5D, 0xA7, 0x89, 0xE3, 0xB1, 0xC2, 0xF0, 0x63, 0xDB, 0x60, 0x68, 0x43, 0x7D, 0x2E, 0x17, 0xAA, 0x71, 0x01, 0xFF, 0x82, 0xB8, 0x38, 0x8D, 0xB0, 0x9F, 0x95, 0x19, 0x39, 0x10, 0xB7, 0x44, 0x7B, 0xBC, 0xD6, 0x64, 0x53, 0xA3, 0x35, 0x7F, 0xF3, 0xD9, 0x72, 0x81, 0xC5, 0x23, 0x10, 0xAC, 0xDF, 0x62, 0x15, 0xD3, 0x8B, 0x61, 0xBF, 0x07, 0x36, 0x51, 0x17, 0x4D, 0x96, 0x3C, 0x3F, 0xF9, 0x5F, 0xEE, 0x4B, 0x29, 0xC8, 0x46, 0xEC, 0xCE, 0x51, 0x1F, 0x8C, 0xC5, 0x83, 0xCD, 0xB5, 0x4E, 0xEE, 0x52, 0x92, 0xE2, 0x95, 0xF1, 0x78, 0x6A, 0x5E, 0x32, 0x1D, 0xB2, 0xF3, 0x52, 0x1F, 0x4B, 0x19, 0x5B, 0xE9, 0xC7, 0x4C, 0x48, 0x30, 0xB4, 0xD7, 0x68, 0x2D, 0x25, 0xB6, 0xD4, 0xF4, 0xCB, 0xDC, 0x82, 0x0F, 0xB2, 0xF6, 0x29, 0x83, 0x62, 0xD7, 0xF5, 0xE3, 0xDC, 0xAC, 0xDC, 0x2A, 0x37, 0x29, 0x7D, 0xC0, 0xBD, 0x0E, 0x4A, 0x88, 0x50, 0x68, 0xBB, 0x9E, 0x86, 0xF6, 0xDC, 0xCF, 0x24, 0xBA, 0xF2, 0xF3, 0x8C, 0x87, 0xBD, 0x60, 0x0B, 0x69, 0x23, 0xD2, 0x00, 0xAD, 0xB8, 0xB3, 0x9E, 0x8A, 0xCF, 0xE9, 0x2F, 0x4B, 0x54, 0x30, 0xCF, 0xC7, 0xCC, 0x16, 0x09, 0xE0, 0x88, 0x2F, 0x6A, 0xCD, 0xA4, 0xB5, 0x62, 0xCD, 0x88, 0xCB, 0xA0, 0xFC, 0x37, 0x6A, 0x16, 0x59, 0x6C, 0x58, 0x1C, 0xF9, 0x58, 0x8B, 0x8E, 0x21, 0x90, 0x95, 0xBB, 0x00, 0x1D, 0x80, 0xC5, 0x74, 0x5A, 0x29, 0xCC, 0x31, 0x22, 0x08, 0xAE, 0xC6, 0xEA, 0x79, 0xEC, 0xE1, 0xAE, 0x93, 0x24, 0xA8, 0x9E, 0x05, 0xD5, 0x46, 0x10, 0xBB, 0xE0, 0xE1, 0xBB, 0x54, 0x51, 0xF3, 0x9E, 0xA2, 0x75, 0xFF, 0xE0, 0x4E, 0x11, 0xAC, 0x3E, 0xCF, 0x30, 0x45, 0x20, 0x54, 0xCF, 0x75, 0xE2, 0x44, 0x99, 0xD6, 0xE2, 0xE5, 0x60, 0x99, 0xA6, 0x99, 0xBC, 0xFA, 0x21, 0xD5, 0xF4, 0x10, 0x6B, 0x1C, 0x3F, 0x15, 0xDF, 0xDD, 0x43, 0xD9, 0x14, 0x50, 0x81, 0x23, 0x25, 0x53, 0xF6, 0x5C, 0x37, 0x93, 0x52, 0xAF, 0x4F, 0xB7, 0x1D, 0x95, 0x28, 0x29, 0xC3, 0x46, 0xE5, 0xB4, 0xDB, 0xB3, 0x8A, 0xCA, 0x7E, 0xF8, 0x72, 0x89, 0xDE, 0x14, 0x3B, 0xD9, 0x94, 0xAD, 0xFE, 0x0D, 0x66, 0xD3, 0x90, 0x94, 0xBD, 0x9F, 0xA6, 0xEF, 0x6C, 0xCC, 0x11, 0x19, 0x25, 0xBF, 0x34, 0xD8, 0x09, 0x7D, 0x9C, 0xDA, 0xCD, 0x75, 0xF0, 0x22, 0x41, 0x22, 0x12, 0x88, 0x76, 0xA1, 0x3D, 0xB8, 0x49, 0x5C, 0xC5, 0xCD, 0x44, 0xA8, 0x99, 0x7D, 0xD6, 0xCA, 0xFF, 0xC1, 0x01, 0xF1, 0x41, 0x77, 0xE0, 0xE2, 0xDF, 0x67, 0xBC, 0x52, 0xA5, 0x9B, 0xAF, 0x48, 0x8E, 0x67, 0xF4, 0x1E, 0x04, 0x54, 0x9D, 0xA8, 0xD4, 0x5D, 0x79, 0xF7, 0x05, 0x3B, 0xE7, 0xFB, 0x0D, 0xB0, 0xCB, 0x97, 0xF4, 0x53, 0x54, 0xA8, 0x40, 0xCA, 0xE2, 0xCD, 0xC4, 0x19, 0x05, 0xE8, 0x9F, 0x6B, 0x25, 0x71, 0x49, 0xC6, 0x34, 0x11, 0x2E, 0x35, 0x58, 0xD0, 0x80, 0xC4, 0x0F, 0x66, 0x1B, 0xE6, 0xE9, 0xC8, 0xF3, 0x92, 0x84, 0xF2, 0x2D, 0x96, 0xBA, 0x89, 0x5F, 0x0D, 0x1E, 0x97, 0x84, 0x4D, 0x89, 0x5D, 0x51, 0x90, 0xAC, 0xAA, 0x2E, 0xBF, 0x2E, 0xB7, 0xF8, 0x16, 0x4B, 0xFE, 0x24, 0x86, 0x1F, 0x11, 0x0F, 0x0A, 0x26, 0x52, 0x55, 0x5E, 0x28, 0x47, 0xDA, 0x87, 0x72, 0xA7, 0xB3, 0x04, 0xB3, 0xBB, 0x7E, 0xAB, 0xA6, 0xE2, 0xDD, 0x6A, 0xA4, 0x30, 0x05, 0x64, 0xBA, 0x15, 0x91, 0x68, 0x6B, 0x3B, 0x46, 0x28, 0x87, 0x6B, 0x84, 0x6C, 0x00, 0xC9, 0x0F, 0x05, 0xF1, 0x15, 0xC6, 0xFA, 0x6C, 0x41, 0xFB, 0xAF, 0x3D, 0x96, 0x42, 0xC6, 0xD1, 0x1B, 0x82, 0x0D, 0x0E, 0x96, 0xB5, 0x08, 0x94, 0x05, 0x54, 0xF7, 0x86, 0xFA, 0xDF, 0xF7, 0xDD, 0xD4, 0x80, 0x82, 0xD1, 0xF7, 0x30, 0xC5, 0x89, 0xB4, 0xC6, 0xC8, 0xCC, 0x98, 0xB5, 0xB7, 0xA5, 0xA4, 0xE3, 0xC2, 0x76, 0x7F, 0x98, 0x45, 0xAE, 0x59, 0x8E, 0xBE, 0x13, 0x5B, 0xDF, 0xB7, 0xEF, 0x6B, 0x30, 0x14, 0x79, 0xC6, 0x4F, 0xBD, 0x40, 0x6C, 0x00, 0x15, 0x7B, 0xC4, 0x0E, 0xBB, 0x6F, 0x04, 0x35, 0x52, 0xC1, 0xF6, 0x40, 0xF0, 0x37, 0xE9, 0x08, 0xAE, 0xC3, 0x26, 0x25, 0xE4, 0xDF, 0xAB, 0xA9, 0x4D, 0xA2, 0xF3, 0x23, 0x1C, 0xF7, 0x77, 0x11, 0x6A, 0x84, 0x94, 0xFB, 0x4D, 0xF2, 0xB1, 0x9D, 0x6E, 0x91, 0xE4, 0xDF, 0x1F, 0x8F, 0x0A, 0x73, 0x47, 0x02, 0x7E, 0xA1, 0x3E, 0x83, 0xBD, 0x2D, 0x46, 0x05, 0xA4, 0x14, 0x09, 0x9D, 0x98, 0x0B, 0x93, 0xF5, 0x33, 0x8C, 0xA7, 0x26, 0x64, 0xAA, 0x85, 0xF3, 0xC3, 0xB4, 0xB0, 0xBB, 0x78, 0x38, 0x95, 0xF3, 0xC7, 0x16, 0x12, 0x80, 0xDC, 0x04, 0x08, 0x2F, 0x77, 0x02, 0x5B, 0x7E, 0x5A, 0x62, 0x17, 0x56, 0x85, 0xF4, 0x6B, 0x1C, 0xDC, 0x41, 0xD5, 0x0C, 0x49, 0x92, 0xF8, 0x35, 0xA1, 0x30, 0x5E, 0x25, 0xD4, 0x9A, 0x9C, 0x8F, 0x0C, 0xF8, 0x50, 0x65, 0x1E, 0xE5, 0x30, 0x45, 0x42, 0xC6, 0xC0, 0x92, 0xD8, 0xBD, 0x87, 0x7C, 0x7D, 0x2A, 0x33, 0x36, 0x65, 0xC7, 0x2C, 0x48, 0x6E, 0x01, 0x55, 0xC9, 0xBC, 0xD1, 0x83, 0x4C, 0xA6, 0x78, 0x68, 0x56, 0x93, 0xD2, 0xC9, 0xAC, 0xD5, 0x6C, 0xCB, 0x7B, 0x09, 0xA1, 0xE7, 0x4A, 0x5F, 0xD1, 0x9C, 0x1F, 0xAC, 0x36, 0xF7, 0x33, 0xA0, 0x64, 0x9D, 0x9E, 0xB6, 0xED, 0x4C, 0xEE, 0x0D, 0xAC, 0x26, 0xFE, 0xB9, 0x50, 0xDD, 0x9F, 0xF9, 0x16, 0x5E, 0xC9, 0x83, 0xC3, 0x08, 0x0A, 0xDE, 0x1F, 0x80, 0x2A, 0x34, 0x26, 0xF3, 0x8D, 0x91, 0x37, 0x14, 0xD6, 0x13, 0xA1, 0xEF, 0xC0, 0x8D, 0x74, 0x19, 0x00, 0xE4, 0x12, 0x59, 0x27, 0xAB, 0x93, 0x05, 0xD7, 0x9E, 0x8C, 0x3B, 0xC8, 0x6E, 0xC4, 0x7F, 0xCE, 0x8A, 0xC7, 0xB5, 0x7D, 0xE5, 0x17, 0x18, 0x5F, 0xB8, 0x90, 0x91, 0x0A, 0x9D, 0xE9, 0xC5, 0x2F, 0x9B, 0x94, 0x60, 0x55, 0x4E, 0x46, 0x3D, 0x5E, 0x69, 0x4B, 0x24, 0xA7, 0x8C, 0xEB, 0x22, 0x13, 0x73, 0x42, 0x80, 0x80, 0x7F, 0x4C, 0x6A, 0x68, 0x78, 0x1F, 0xEE, 0xB9, 0x15, 0x98, 0x22, 0xF5, 0xAE, 0x13, 0x8C, 0xCC, 0xB1, 0x18, 0xBC, 0xEA, 0xE3, 0x83, 0xFA, 0x87, 0xE4, 0x61, 0x78, 0x6D, 0x1B, 0x6C, 0xC1, 0xB9, 0xBC, 0x65, 0xDE, 0x3E, 0x9A, 0xA5, 0x88, 0xB7, 0xFC, 0x2B, 0x24, 0x38, 0x5C, 0xFB, 0xB5, 0x15, 0xE9, 0x4C, 0x01, 0xD4, 0xE4, 0x9C, 0x04, 0xA2, 0x6E, 0xA0, 0x65, 0x1C, 0x85, 0x00, 0x86, 0xF9, 0x3B, 0x4B, 0xA3, 0x80, 0x90, 0xBC, 0x8C, 0x30, 0xF4, 0x4F, 0xC8, 0xB9, 0x42, 0x49, 0x90, 0xA8, 0x21, 0xE7, 0x7C, 0x17, 0x94, 0xEA, 0x6C, 0x2C, 0x5A, 0x9E, 0x07, 0xDF, 0xDF, 0x5E, 0xD9, 0x7E, 0x40, 0x55, 0x86, 0x18, 0x61, 0x0A, 0x8D, 0xAF, 0xB1, 0xA2, 0x61, 0xAB, 0x69, 0x10, 0xB2, 0x50, 0x3A, 0xD9, 0x84, 0x93, 0x25, 0xBE, 0x13, 0x7F, 0xD3, 0x6E, 0x49, 0xFE, 0xD4, 0xB6, 0x2B, 0x20, 0x85, 0x09, 0xA4, 0xE0, 0x7E, 0x8F, 0xF3, 0xAA, 0xE0, 0x8A, 0x7F, 0xA5, 0x61, 0x5B, 0x8D, 0xBA, 0xD8, 0x7D, 0x1F, 0xD0, 0xE0, 0x7B, 0x4F, 0xA3, 0xD2, 0x89, 0xF0, 0xC1, 0x90, 0x31, 0x50, 0x40, 0x83, 0x55, 0x9F, 0x52, 0x99, 0x26, 0xDC, 0x47, 0x21, 0xA3, 0x29, 0x60, 0x33, 0xE1, 0x8B, 0x07, 0xFE, 0x35, 0x8F, 0xA6, 0x83, 0x20, 0xCD, 0x05, 0x95, 0x58, 0xB5, 0xCC, 0x72, 0x87, 0x85, 0x27, 0x81, 0x8F, 0xBF, 0x7D, 0x15, 0xEC, 0xC0, 0xDE, 0x30, 0x75, 0xDE, 0x2C, 0xF3, 0x1F, 0x16, 0x0E, 0xB4, 0xDD, 0x4C, 0xAF, 0x7B, 0xB3, 0xF1, 0x8B, 0x11, 0x2C, 0x1D, 0x70, 0xE9, 0x34, 0xEC, 0x0F, 0x01, 0xC2, 0xCC, 0x9B, 0xCA, 0x48, 0xEF, 0xD7, 0x63, 0x6D, 0x69, 0x51, 0xF6, 0xFC, 0xD9, 0x2B, 0x5F, 0x18, 0xE2, 0x46, 0xF3, 0xFC, 0x81, 0x9A, 0x09, 0xDB, 0x12, 0xBA, 0xDC, 0xDD, 0x10, 0x4E, 0x81, 0x56, 0x18, 0x09, 0xF3, 0x17, 0xFC, 0xCC, 0x93, 0x12, 0x93, 0x1A, 0x9A, 0x5C, 0x3D, 0xA6, 0x57, 0x50, 0x90, 0xF5, 0xAA, 0xFA, 0x39, 0xEB, 0xE1, 0x3E, 0x19, 0x4D, 0xD9, 0x0B, 0xAB, 0x8B, 0xB8, 0x25, 0x67, 0x93, 0x27, 0x93, 0xFD, 0x7D, 0x1D, 0x91, 0x6C, 0x0C, 0xE3, 0x94, 0x22, 0x12, 0x3B, 0x02, 0x0E, 0xB1, 0xCD, 0xAB, 0x05, 0x6F, 0xA9, 0xA7, 0x87, 0x01, 0xB4, 0xD3, 0x40, 0x6D, 0xF1, 0x40, 0x32, 0x11, 0x05, 0xE5, 0xAF, 0x88, 0xE3, 0x9E, 0xD5, 0x28, 0x8A, 0xDC, 0x35, 0xBB, 0x75, 0x21, 0xE4, 0x58, 0x67, 0x66, 0xDE, 0x04, 0x84, 0xE5, 0x7D, 0x3E, 0xEF, 0x14, 0x0C, 0x9A, 0xDB, 0xB4, 0xA9, 0x7D, 0x65, 0xD0, 0xEB, 0xF4, 0x36, 0x50, 0x2B, 0x50, 0x86, 0xA9, 0x82, 0x7C, 0xB1, 0x61, 0x45, 0xE9, 0xAF, 0x49, 0xCF, 0xFD, 0x11, 0x21, 0xAC, 0x98, 0x01, 0x41, 0xB3, 0xEC, 0x23, 0x1B, 0x66, 0xAF, 0x2A, 0x61, 0x34, 0x89, 0x9B, 0x88, 0x54, 0xDF, 0x4D, 0xF1, 0xE9, 0xDE, 0x5E, 0xB0, 0xEB, 0x81, 0x08, 0x1C, 0x17, 0x1E, 0xBD, 0x23, 0x8D, 0x82, 0x1A, 0x6D, 0x83, 0x75, 0x63, 0x61, 0x32, 0x32, 0x3C, 0x30, 0x5C, 0x08, 0xE3, 0x78, 0xF4, 0x96, 0x32, 0xBD, 0x8A, 0x16, 0x21, 0xDD, 0x6F, 0x42, 0x4E, 0x3D, 0x68, 0xF6, 0x6E, 0xB5, 0x27, 0x67, 0x03, 0x1E, 0x99, 0x27, 0x84, 0x82, 0x6C, 0xFE, 0xC6, 0x06, 0x0F, 0x8F, 0xDB, 0x6B, 0xD2, 0x87, 0xBE, 0xE8, 0x3E, 0xF5, 0xFD, 0xD6, 0xFA, 0x26, 0x4B, 0xB8, 0xC1, 0xBE, 0x9D, 0xC4, 0xF3, 0x61, 0x61, 0xEE, 0x15, 0xC1, 0xEC, 0x75, 0xD8, 0xC2, 0x75, 0xC1, 0xD8, 0x02, 0x5B, 0x65, 0x77, 0xD3, 0xBF, 0x38, 0xC5, 0x63, 0xE7, 0xF5, 0x5A, 0xAA, 0x82, 0x10, 0x04, 0x63, 0x51, 0xF4, 0x0D, 0xE2, 0x2A, 0x54, 0x47, 0x21, 0x0E, 0x68, 0xB3, 0xB5, 0x77, 0xB2, 0x13, 0x42, 0xAE, 0xEC, 0xD4, 0x50, 0x0A, 0xC2, 0x4A, 0x63, 0xB2, 0xFE, 0xDE, 0x3A, 0x98, 0xB4, 0x87, 0x8A, 0xA5, 0xD2, 0x1D, 0xCA, 0xE8, 0x62, 0x14, 0x74, 0xD9, 0x20, 0x32, 0xE4, 0x20, 0xC5, 0xB9, 0x25, 0x1A, 0xE5, 0x54, 0xCF, 0x05, 0xA3, 0x0B, 0x22, 0xAB, 0x65, 0x58, 0xA2, 0xD9, 0x57, 0xEF, 0xC0, 0x8A, 0x96, 0x57, 0xC2, 0xA8, 0x41, 0x11, 0x80, 0x78, 0xAA, 0x91, 0x04, 0x5A, 0xA9, 0xFE, 0x67, 0xF4, 0x29, 0x3E, 0x45, 0xAC, 0xCB, 0xC4, 0xB1, 0x95, 0x73, 0x69, 0x49, 0x3B, 0x7E, 0xAA, 0x5A, 0xD7, 0x94, 0x4A, 0xBC, 0xCD, 0x42, 0x1F, 0x81, 0x16, 0x45, 0x5F, 0x71, 0xC1, 0x03, 0xD4, 0xD3, 0x6E, 0xCA, 0xBB, 0xAC, 0xC3, 0x42, 0x9F, 0xE7, 0x64, 0xE9, 0x5B, 0x92, 0x83, 0x66, 0xFC, 0xE1, 0xEA, 0x16, 0xC0, 0x2A, 0x7F, 0x2C, 0xED, 0x06, 0xCF, 0xB4, 0xD8, 0xC2, 0x91, 0x53, 0x34, 0x97, 0xF9, 0x57, 0xB2, 0x46, 0xE3, 0xD9, 0x1C, 0x1C, 0x8E, 0xB7, 0x51, 0x20, 0x3B, 0xFF, 0x29, 0x3D, 0x5A, 0x40, 0x79, 0xDA, 0xDE, 0x30, 0xA0, 0x6D, 0x39, 0x32, 0x1A, 0x35, 0x43, 0x77, 0xBF, 0x66, 0xE2, 0x64, 0xE1, 0x9D, 0xD8, 0x37, 0xF2, 0xFE, 0x75, 0x3A, 0x3D, 0x75, 0x5A, 0x2C, 0x84, 0xF1, 0xD3, 0x9C, 0x4F, 0x52, 0xCF, 0xB5, 0x30, 0x83, 0x79, 0x54, 0xDD, 0x2C, 0xD9, 0xFC, 0x7C, 0xBC, 0xDE, 0xA1, 0x51, 0xD6, 0x21, 0xE2, 0x4D, 0xD0, 0x45, 0xB3, 0x7F, 0x1F, 0xC6, 0x70, 0x22, 0xA9, 0x4E, 0x7D, 0xC1, 0x31, 0xD6, 0x3A, 0x43, 0x95, 0x3D, 0xE3, 0x31, 0xE3, 0xF6, 0x5A, 0x88, 0xED, 0x87, 0xDA, 0x10, 0x84, 0xA3, 0x80, 0x33, 0x28, 0xC7, 0xED, 0x6A, 0x99, 0x59, 0x7E, 0x89, 0x10, 0x12, 0xBA, 0xE4, 0x41, 0x76, 0x59, 0xE4, 0x08, 0x74, 0x83, 0xD5, 0xCC, 0xAB, 0x20, 0x5E, 0xCC, 0xDB, 0xD3, 0x36, 0xF6, 0x89, 0x41, 0xF3, 0xB3, 0xED, 0x99, 0xEB, 0xDD, 0xFD, 0x3A, 0xD0, 0x2C, 0xCE, 0x97, 0x8D, 0x73, 0x29, 0x08, 0x95, 0x92, 0x45, 0x4D, 0x20, 0x06, 0x59, 0x27, 0x41, 0x12, 0x71, 0x61, 0x1F, 0xC6, 0x6F, 0x16, 0x38, 0x6F, 0x20, 0x4B, 0xF9, 0xEF, 0x90, 0x2E, 0xC8, 0xD7, 0xBF, 0x19, 0x8B, 0xF5, 0xF8, 0xFA, 0x3D, 0x11, 0x49, 0xE4, 0xD6, 0xE3, 0x2E, 0x5C, 0xFE, 0xD8, 0x50, 0x78, 0x0B, 0x94, 0xDD, 0x4C, 0x60, 0x50, 0x34, 0x4F, 0x1D, 0x57, 0xCF, 0x48, 0x56, 0x6D, 0x9D, 0xD3, 0x5A, 0x59, 0x21, 0xEC, 0x36, 0xC0, 0xD3, 0x66, 0x56, 0x2A, 0xA1, 0x42, 0x99, 0x73, 0xD4, 0xE6, 0xD3, 0x80, 0xF8, 0x9E, 0x17, 0xF1, 0x53, 0xC6, 0xC0, 0xBE, 0x75, 0x03, 0x17, 0xCE, 0x60, 0xC7, 0x16, 0x26, 0x81, 0x7E, 0xA4, 0xE1, 0x78, 0x83, 0x0E, 0x10, 0x52, 0xDC, 0xF9, 0x22, 0x5F, 0xAE, 0x00, 0x9C, 0x1F, 0x0B, 0x36, 0xAD, 0xD4, 0xA2, 0xDB, 0x3D, 0xB4, 0x3C, 0xFF, 0x55, 0x6E, 0x82, 0xCF, 0x48, 0x52, 0x32, 0xF1, 0xB9, 0x5B, 0x6B, 0xEC, 0x08, 0x8F, 0xBF, 0x7A, 0x3D, 0x0C, 0x6A, 0x06, 0x83, 0x5F, 0xFC, 0xFF, 0xC7, 0x1D, 0xBF, 0xA7, 0x7C, 0x63, 0xE1, 0x88, 0x4A, 0x1D, 0xD4, 0x72, 0x8B, 0x53, 0x22, 0x8A, 0x7C, 0xB7, 0x50, 0xB7, 0x10, 0xD5, 0x96, 0xE3, 0x54, 0x26, 0x21, 0x5B, 0x99, 0xD9, 0xC3, 0x60, 0x06, 0x93, 0x3A, 0xB3, 0x9C, 0xC8, 0xF1, 0x6F, 0x65, 0xBF, 0x79, 0xAA, 0xBA, 0x6B, 0x63, 0xB6, 0xC9, 0x23, 0x18, 0x47, 0x38, 0xE1, 0x72, 0x02, 0x5C, 0x00, 0x2E, 0x9D, 0xF8, 0xBD, 0x00, 0xD9, 0x98, 0x19, 0x30, 0xCB, 0xD2, 0xD4, 0x13, 0x49, 0x70, 0xE8, 0x0C, 0x82, 0x5C, 0xEB, 0xB7, 0x87, 0x20, 0x08, 0x62, 0xAD, 0xC5, 0x35, 0x51, 0xA8, 0xFA, 0x71, 0xA3, 0x8C, 0xC5, 0x2C, 0x7E, 0xBB, 0x25, 0xC4, 0xF8, 0x29, 0x54, 0x7A, 0xEC, 0xD0, 0x44, 0x7F, 0x52, 0x9D, 0x2D, 0x6C, 0xF3, 0x2C, 0xB9, 0x80, 0xC4, 0xF8, 0x47, 0xDC, 0x33, 0x75, 0xA6, 0x04, 0x7A, 0x96, 0xF2, 0x51, 0x44, 0x2E, 0x81, 0x0F, 0xF1, 0x27, 0xF2, 0x3A, 0x6C, 0x63, 0xE4, 0xE3, 0x64, 0x27, 0x53, 0xC2, 0x09, 0x96, 0xDE, 0x5D, 0xB2, 0x57, 0x7B, 0x54, 0x0B, 0x07, 0x13, 0x2C, 0x0A, 0x1A, 0xCB, 0x3A, 0xF4, 0x3C, 0x5B, 0x1E, 0x0E, 0x9B, 0x85, 0x21, 0x3F, 0x34, 0xE2, 0x64, 0xB2, 0x30, 0x62, 0x46, 0xA1, 0x7C, 0x83, 0xF5, 0x0C, 0x6F, 0xE9, 0x6E, 0x67, 0x25, 0x8B, 0x95, 0xB6, 0xA9, 0x9C, 0x22, 0xBD, 0x9A, 0x6E, 0x66, 0xD6, 0x0A, 0xDE, 0x28, 0x35, 0x63, 0xD8, 0x1B, 0xA5, 0x04, 0x03, 0x82, 0xC7, 0x22, 0x30, 0x75, 0xD4, 0xDD, 0x2C, 0xE6, 0xF9, 0xDF, 0x7D, 0xD8, 0xB3, 0x9C, 0x7F, 0x23, 0x14, 0x73, 0x98, 0xD7, 0xA0, 0x01, 0x37, 0x6E, 0x77, 0xCE, 0x62, 0x06, 0xAC, 0xC9, 0xD3, 0xF4, 0x86, 0xA6, 0x9B, 0xDD, 0xD8, 0x08, 0x41, 0x74, 0x52, 0xE8, 0x3A, 0xC6, 0xBB, 0x3C, 0xEA, 0x14, 0xE3, 0x26, 0x87, 0x34, 0x49, 0xD4, 0x28, 0x42, 0xFB, 0x2E, 0x06, 0x2F, 0xD3, 0xB6, 0x05, 0x69, 0xC6, 0x6C, 0x74, 0xEE, 0x88, 0x94, 0x6F, 0x2A, 0x9D, 0x18, 0xC5, 0x8A, 0x1C, 0x21, 0xC8, 0x77, 0xE9, 0x1A, 0x37, 0x27, 0xD6, 0x26, 0x2D, 0x03, 0x4E, 0xC0, 0x87, 0xC3, 0xA4, 0x18, 0xFA, 0x07, 0xE1, 0xED, 0x8F, 0xFF, 0x1B, 0x3D, 0x74, 0x1E, 0x08, 0x8A, 0x5F, 0x52, 0xD3, 0x66, 0xF9, 0xBF, 0x1B, 0x86, 0x94, 0x99, 0x03, 0xC6, 0x58, 0xC1, 0x8C, 0x96, 0x34, 0x28, 0x1B, 0x41, 0xCC, 0x45, 0xA4, 0x16, 0xA1, 0x4B, 0xBB, 0x21, 0x32, 0xA6, 0x51, 0xF2, 0xBB, 0x46, 0xA0, 0xEE, 0x78, 0x53, 0x5F, 0xD7, 0x92, 0x5B, 0x0A, 0xD0, 0xD3, 0xF7, 0xD8, 0x50, 0xC4, 0xAC, 0x9E, 0x54, 0x90, 0xEA, 0x70, 0xE1, 0x16, 0x4E, 0x9E
        };


        #endregion

        #region Constructor
        public SessionKeyGenerator()
        {
        }
        #endregion

        /// <summary>
        /// 取得16 byte的隨機值
        /// </summary>
        /// <param name="startIndex">指定的起始位置</param>
        /// <returns>16byte的隨機值</returns>
        public byte[] GetRanA(int startIndex)
        {
            if (startIndex < 0 || startIndex > (SessionKeyGenerator._randData.Length - SessionKeyGenerator.DataLength))
            {
                throw new ArgumentOutOfRangeException("specified index out of range(max:" + SessionKeyGenerator._randData.Length + ") : " + startIndex);
            }
            byte[] result = new byte[SessionKeyGenerator.DataLength];
            Buffer.BlockCopy(SessionKeyGenerator._randData, startIndex, result, 0, SessionKeyGenerator.DataLength);
            return result;
        }


        /// <summary>
        /// 取得隨機陣列總長度
        /// </summary>
        /// <returns>Ran array Total Length</returns>
        public int GetTotalLength()
        {
            return SessionKeyGenerator._randData.Length;
        }

        /// <summary>
        /// 產生指定大小的隨機byte數據檔案
        /// </summary>
        /// <param name="randomCount">byte數量</param>
        protected void WriteFile(int randomCount)
        {
            RandomNumberGenerator ranObj = new RNGCryptoServiceProvider();
            byte[] randomData = new byte[randomCount];
            string filePath = AppDomain.CurrentDomain.BaseDirectory + @"\Random.txt";
            // 1. Byte Array產生Random數據
            ranObj.GetBytes(randomData);
            ranObj.Dispose();
            // 2. 寫入字串並稍為改寫字串表達意義
            StringBuilder ranStr = new StringBuilder();
            foreach(byte b in randomData)
            {
                ranStr.Append(String.Format("0x{0:X2}, ", b));
            }
            // 3. 寫入檔案流中
            using(FileStream fs = new FileStream(filePath,FileMode.Create,FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(ranStr.ToString(0, ranStr.Length - 2));//多兩個字串 => ", " 所以扣2
                    sw.Flush();
                }
            }
        }

        /// <summary>
        /// 取得Session Key(16 bytes)
        /// SesKey = rndA[0..3] || rndB[0..3] || rndA[12..15] || rndB[12..15]
        /// </summary>
        /// <param name="ranAStartIndex">Random A Start Index</param>
        /// <param name="ranB">Random B</param>
        /// <returns>Session Key(16 bytes)</returns>
        public byte[] GetSessionKey(int ranAStartIndex, byte[] ranB)
        {
            byte[] ranA = this.GetRanA(ranAStartIndex);
            byte[] sessionKey = new byte[DataLength];
            
            Buffer.BlockCopy(ranA, 0, sessionKey, 0, 4);
            Buffer.BlockCopy(ranB, 0, sessionKey, 4, 4);
            Buffer.BlockCopy(ranA, 12, sessionKey, 8, 4);
            Buffer.BlockCopy(ranB, 12, sessionKey, 12, 4);
            return sessionKey;
        }
    }
}
