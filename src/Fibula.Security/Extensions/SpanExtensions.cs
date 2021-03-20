﻿// -----------------------------------------------------------------
// <copyright file="SpanExtensions.cs" company="2Dudes">
// Copyright (c) | Jose L. Nunez de Caceres et al.
// https://linkedin.com/in/nunezdecaceres
//
// All Rights Reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
// -----------------------------------------------------------------

namespace Fibula.Security.Extensions
{
    using System;

    /// <summary>
    /// Static class that provides security-related helper methods for <see cref="Span{T}"/> structures.
    /// </summary>
    public static class SpanExtensions
    {
        /// <summary>
        /// Encrypts a byte range.
        /// </summary>
        /// <param name="buffer">The bytes to encrypt.</param>
        /// <param name="key">The encryption key bytes.</param>
        /// <param name="messageLength">The resulting length of the buffer.</param>
        /// <returns>True if encryption was successful, false otherwise.</returns>
        public static unsafe bool XteaEncrypt(this Span<byte> buffer, uint[] key, out int messageLength)
        {
            messageLength = buffer.Length;

            if (key == null)
            {
                return false;
            }

            int pad = messageLength % 8;

            if (pad > 0)
            {
                messageLength += 8 - pad;
            }

            fixed (byte* bufferPtr = buffer)
            {
                uint* words = (uint*)bufferPtr;

                for (int pos = 0; pos < messageLength / 4; pos += 2)
                {
                    uint xSum = 0, xDelta = 0x9e3779b9, xCount = 32;

                    while (xCount-- > 0)
                    {
                        words[pos] += (words[pos + 1] << 4 ^ words[pos + 1] >> 5) + words[pos + 1] ^ xSum + key[xSum & 3];

                        xSum += xDelta;

                        words[pos + 1] += (words[pos] << 4 ^ words[pos] >> 5) + words[pos] ^ xSum + key[xSum >> 11 & 3];
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Decrypts a byte range.
        /// </summary>
        /// <param name="buffer">The bytes to decrypt.</param>
        /// <param name="key">The decryption key bytes.</param>
        /// <param name="messageLength">The resulting length of the buffer.</param>
        /// <returns>True if decryption was successful, false otherwise.</returns>
        public static unsafe bool XteaDecrypt(this Span<byte> buffer, uint[] key, out int messageLength)
        {
            messageLength = buffer.Length;

            if (messageLength % 8 > 0 || key == null)
            {
                return false;
            }

            fixed (byte* bufferPtr = buffer)
            {
                uint* words = (uint*)bufferPtr;

                for (int pos = 0; pos < messageLength / 4; pos += 2)
                {
                    uint xCount = 32, xSum = 0xC6EF3720, xDelta = 0x9E3779B9;

                    while (xCount-- > 0)
                    {
                        words[pos + 1] -= (words[pos] << 4 ^ words[pos] >> 5) + words[pos] ^ xSum + key[xSum >> 11 & 3];

                        xSum -= xDelta;

                        words[pos] -= (words[pos + 1] << 4 ^ words[pos + 1] >> 5) + words[pos + 1] ^ xSum + key[xSum & 3];
                    }
                }
            }

            messageLength = BitConverter.ToUInt16(buffer) + 2;

            return true;
        }
    }
}
