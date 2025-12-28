// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Overrides.Blocks;

public static class BlockListExtensions
{
    extension(List<Block> blocks)
    {
        /// <summary>
        /// Normalize indexes relative to plain text in the event,
        /// excluding Override, Comment, and Drawing blocks
        /// </summary>
        /// <remarks>
        /// Given <c>Hello {\b1}World{\b0}!</c>, origin index 13 ("r"),
        /// normalized index 8 will be returned.
        /// </remarks>
        /// <param name="originIndex">Original index</param>
        /// <returns>Normalized index</returns>
        public int NormalizeIndex(int originIndex)
        {
            var remaining = originIndex;
            var plainLength = 0;

            for (var i = 0; i < blocks.Count && remaining > 0; i++)
            {
                var block = blocks[i];
                var blockLength = block.Text.Length;

                if (block is OverrideBlock or CommentBlock)
                {
                    remaining -= blockLength;
                    continue;
                }

                var consumed = Math.Min(blockLength, remaining);
                plainLength += consumed;
                remaining -= consumed;
            }

            return plainLength;
        }

        /// <summary>
        /// Normalize indexes relative to the given block
        /// </summary>
        /// <remarks>
        /// Given <c>Hello {\b1}World{\b0}!</c>, origin index 13 ("r"),
        /// normalized index 2 will be returned.
        /// </remarks>
        /// <param name="plainBlockIdx">Plain text block to normalize to</param>
        /// <param name="originIndex">Original index</param>
        /// <returns>Normalized index</returns>
        public int NormalizeIndex(int plainBlockIdx, int originIndex)
        {
            var remaining = originIndex;

            for (var i = 0; i < blocks.Count && remaining > 0; i++)
            {
                var block = blocks[i];
                var blockLength = block.Text.Length;

                if (block is OverrideBlock or CommentBlock)
                {
                    remaining -= blockLength;
                    continue;
                }

                var consumed = Math.Min(blockLength, remaining);

                // If this is the block we're targeting, return index within it
                if (i == plainBlockIdx)
                    return consumed;

                remaining -= consumed;
            }

            return remaining;
        }

        /// <summary>
        /// Get the block index for the text at the index
        /// </summary>
        /// <param name="normalizedIndex">Normalized index in the text to look up</param>
        /// <returns>Block index</returns>
        public int NormalizedBlockAt(int normalizedIndex)
        {
            var remaining = normalizedIndex;
            var blockIdx = 0;
            var blockCount = blocks.Count;

            for (var i = 0; i < blockCount; i++)
            {
                var block = blocks[i];
                var hasNext = i + 1 < blockCount;

                // Braced blocks don't contribute to the normalized text index
                if (IsBraced(block))
                {
                    if (i > 0 && remaining >= 0)
                        blockIdx++;

                    if (remaining > 0 && (!hasNext || !IsBraced(blocks[i + 1])))
                        blockIdx++;
                    continue;
                }

                remaining -= block.Text.Length;
                switch (remaining)
                {
                    case < 0:
                        return blockIdx;
                    case 0:
                        return blockIdx + (hasNext && IsBraced(blocks[i + 1]) ? 1 : 0);
                }
            }
            return blockIdx;

            bool IsBraced(Block block) => block is OverrideBlock or CommentBlock;
        }

        /// <summary>
        /// Find the tag with the given name
        /// </summary>
        /// <param name="blockIdx">Block index to check</param>
        /// <param name="tagName">Name of the tag</param>
        /// <param name="alt">Alternate name for the tag</param>
        /// <returns>The tag, or <see langword="null"/> if not found</returns>
        public OverrideTag? FindTag(int blockIdx, string tagName, string alt = "")
        {
            if (blockIdx >= blocks.Count)
                blockIdx = blocks.Count - 1;

            for (var b = blockIdx; b >= 0; b--)
            {
                if (blocks[b] is not OverrideBlock block)
                    continue;

                for (var t = block.Tags.Count - 1; t >= 0; t--)
                {
                    if (block.Tags[t].Name == tagName || block.Tags[t].Name == alt)
                        return block.Tags[t];
                }
            }
            return null;
        }

        /// <summary>
        /// Set the value of a tag
        /// </summary>
        /// <param name="tag">Tag to set</param>
        /// <param name="normPos">Normalized position</param>
        /// <param name="originPos">Original position</param>
        /// <exception cref="ArgumentOutOfRangeException">If the <see cref="BlockType"/> is invalid</exception>
        /// <returns>Number of characters to shift the caret</returns>
        public int SetTag(OverrideTag tag, int normPos, int originPos)
        {
            var start = NormalizedBlockAt(blocks, normPos);
            OverrideBlock? ovr = null;
            PlainBlock? plainBlock = null;
            var insertIdx = -1;

            for (var i = start; i >= 0; i--)
            {
                switch (blocks[i].Type)
                {
                    case BlockType.Comment:
                    case BlockType.Drawing:
                        continue;
                    case BlockType.Override:
                        ovr = blocks[i] as OverrideBlock;
                        goto found;
                    case BlockType.Plain:
                        plainBlock = blocks[i] as PlainBlock;
                        insertIdx = originPos;
                        start = i;
                        goto found;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tag), tag, null);
                }
            }

            found:
            if (insertIdx < 0 && ovr is null)
                insertIdx = 0;

            var shift = tag.ToString().Length;

            // Modify existing OverrideBlock
            if (ovr is not null)
            {
                var alt = tag.Name switch
                {
                    OverrideTags.C => OverrideTags.C1,
                    OverrideTags.C1 => OverrideTags.C,
                    OverrideTags.Fr => OverrideTags.FrZ,
                    OverrideTags.FrZ => OverrideTags.Fr,
                    _ => string.Empty,
                };
                var foundTag = false;

                for (var i = ovr.Tags.Count - 1; i >= 0; i--)
                {
                    var name = ovr.Tags[i].Name;
                    if (name != tag.Name && name != alt)
                        continue;

                    shift -= ovr.Tags[i].ToString().Length;

                    if (!foundTag)
                    {
                        ovr.Tags[i] = tag;
                        foundTag = true;
                    }
                    else
                    {
                        ovr.Tags.RemoveAt(i);
                    }
                }

                if (!foundTag)
                    ovr.Tags.Add(tag);
            }
            // Add a new OverrideBlock
            else if (plainBlock is not null)
            {
                ovr = new OverrideBlock(Span<char>.Empty);
                ovr.Tags.Add(tag);

                // Split the plain block
                var normBlockIdx = NormalizeIndex(blocks, start, insertIdx);
                var left = new PlainBlock(plainBlock.Text[..normBlockIdx]);
                var right = new PlainBlock(plainBlock.Text[normBlockIdx..]);

                blocks.RemoveAt(start);
                blocks.InsertRange(start, [left, ovr, right]);
                shift += 2; // Account for {}
            }
            return shift;
        }
    }
}
