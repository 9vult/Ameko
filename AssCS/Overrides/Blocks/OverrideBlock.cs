// SPDX-License-Identifier: MPL-2.0

using AssCS.Utilities;

namespace AssCS.Overrides.Blocks;

/// <summary>
/// An override block
/// </summary>
/// <remarks>
/// An override block is composed of a set
/// of override brackets <c>{ }</c> containing
/// one or more override tags
/// </remarks>
public class OverrideBlock(ReadOnlySpan<char> data) : Block(data.ToString(), BlockType.Override)
{
    /// <summary>
    /// Override tags in this block
    /// </summary>
    public List<OverrideTag> Tags { get; } = ParseTags(data);

    public override string Text
    {
        get
        {
            _text = $"{{{string.Join("", Tags.Select(t => t.ToString()))}}}";
            return _text;
        }
    }

    private static List<OverrideTag> ParseTags(ReadOnlySpan<char> data)
    {
        List<OverrideTag> tags = [];
        while (!data.IsEmpty)
        {
            var q = data.IndexOf('\\');
            if (q < 0)
                q = data.Length;
            data = data[q..];

            if (data[0] is not '\\')
                break;

            // Get the name of the tag
            // Move in from the \, trimming any leading whitespace
            data = data[1..].TrimStart();

            for (q = 0; q < data.Length && data[q] is not ('(' or '\\'); q++) { }
            if (q == 0)
                continue;

            var tagName = data[..q].ToString();
            data = data[q..];

            const int maxValidArgCount = 7;
            var args = new List<string>(maxValidArgCount + 1);
            var hasBackslashArg = false;
            // Split parenthesized arguments
            if (data[0] is '(')
            {
                data = data[1..];

                while (true)
                {
                    data = data.TrimStart(); // Strip whitespace, if any

                    for (q = 0; q < data.Length && data[q] is not (',' or '\\' or ')'); q++) { }

                    var r = data[q];
                    if (r is ',')
                    {
                        args.Add(data[..q].ToString());
                    }
                    else
                    {
                        // Swallow the rest of the string
                        // Could be either a backslash arg or last arg
                        if (r is '\\')
                        {
                            hasBackslashArg = true;
                            q = data.IndexOf(')');
                            if (q < 0)
                                q = data.Length;
                        }
                        args.Add(data[..q].ToString());

                        // Closing parenthesis could be missing
                        if (q != data.Length)
                            data = data[q..];
                        break;
                    }
                }
            }

            // Tag/Arg parsing time
            switch (tagName)
            {
                case OverrideTags.A:
                    if (args.Count > 0)
                        tags.Add(new OverrideTag.A(args[0].ParseAssInt()));
                    else
                        tags.Add(new OverrideTag.Unknown(OverrideTags.A));
                    break;
                case OverrideTags.A1:
                    tags.Add(new OverrideTag.A1(args.Count > 0 ? args[0] : null));
                    break;
                case OverrideTags.A2:
                    tags.Add(new OverrideTag.A2(args.Count > 0 ? args[0] : null));
                    break;
                case OverrideTags.A3:
                    tags.Add(new OverrideTag.A3(args.Count > 0 ? args[0] : null));
                    break;
                case OverrideTags.A4:
                    tags.Add(new OverrideTag.A4(args.Count > 0 ? args[0] : null));
                    break;
                case OverrideTags.Alpha:
                    tags.Add(new OverrideTag.Alpha(args.Count > 0 ? args[0] : null));
                    break;
                case OverrideTags.An:
                    tags.Add(new OverrideTag.An(args.Count > 0 ? args[0].ParseAssInt() : null));
                    break;
                case OverrideTags.B:
                    tags.Add(new OverrideTag.B(args.Count > 0 ? args[0].ParseAssInt() == 1 : null));
                    break;
                case OverrideTags.Be:
                    tags.Add(new OverrideTag.Be(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.Blur:
                    tags.Add(
                        new OverrideTag.Blur(args.Count > 0 ? args[0].ParseAssDouble() : null)
                    );
                    break;
                case OverrideTags.Bord:
                    tags.Add(
                        new OverrideTag.Bord(args.Count > 0 ? args[0].ParseAssDouble() : null)
                    );
                    break;
                case OverrideTags.C:
                    tags.Add(new OverrideTag.C(args.Count > 0 ? args[0] : null));
                    break;
                case OverrideTags.C1:
                    tags.Add(new OverrideTag.C1(args.Count > 0 ? args[0] : null));
                    break;
                case OverrideTags.C2:
                    tags.Add(new OverrideTag.C2(args.Count > 0 ? args[0] : null));
                    break;
                case OverrideTags.C3:
                    tags.Add(new OverrideTag.C3(args.Count > 0 ? args[0] : null));
                    break;
                case OverrideTags.C4:
                    tags.Add(new OverrideTag.C4(args.Count > 0 ? args[0] : null));
                    break;
                case OverrideTags.Clip:
                    if (args.Count == 4)
                        tags.Add(
                            new OverrideTag.Clip(
                                args[0].ParseAssInt(),
                                args[1].ParseAssInt(),
                                args[2].ParseAssInt(),
                                args[3].ParseAssInt()
                            )
                        );
                    else
                        tags.Add(new OverrideTag.Unknown(OverrideTags.Clip, args.ToArray()));
                    break;
                case OverrideTags.Fad:
                    if (args.Count == 2)
                        tags.Add(new OverrideTag.Fad(args[0].ParseAssInt(), args[1].ParseAssInt()));
                    else
                        tags.Add(new OverrideTag.Unknown(OverrideTags.Fad, args.ToArray()));
                    break;
                case OverrideTags.Fade:
                    if (args.Count == 7)
                        tags.Add(
                            new OverrideTag.Fade(
                                args[0].ParseAssInt(),
                                args[1].ParseAssInt(),
                                args[2].ParseAssInt(),
                                args[3].ParseAssInt(),
                                args[4].ParseAssInt(),
                                args[5].ParseAssInt(),
                                args[6].ParseAssInt()
                            )
                        );
                    else
                        tags.Add(new OverrideTag.Unknown(OverrideTags.Fade, args.ToArray()));
                    break;
                case OverrideTags.FaX:
                    tags.Add(new OverrideTag.FaX(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.FaY:
                    tags.Add(new OverrideTag.FaY(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.Fe:
                    tags.Add(new OverrideTag.Fe(args.Count > 0 ? args[0].ParseAssInt() : null));
                    break;
                case OverrideTags.Fn:
                    tags.Add(new OverrideTag.Fn(args.Count > 0 ? args[0] : null));
                    break;
                case OverrideTags.Fr:
                    tags.Add(new OverrideTag.Fr(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.FrX:
                    tags.Add(new OverrideTag.FrX(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.FrY:
                    tags.Add(new OverrideTag.FrY(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.FrZ:
                    tags.Add(new OverrideTag.FrZ(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.Fs:
                    tags.Add(new OverrideTag.Fs(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.Fsc:
                    tags.Add(new OverrideTag.Fsc());
                    break;
                case OverrideTags.FscX:
                    tags.Add(
                        new OverrideTag.FscX(args.Count > 0 ? args[0].ParseAssDouble() : null)
                    );
                    break;
                case OverrideTags.FscY:
                    tags.Add(
                        new OverrideTag.FscY(args.Count > 0 ? args[0].ParseAssDouble() : null)
                    );
                    break;
                case OverrideTags.Fsp:
                    tags.Add(new OverrideTag.Fsp(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.I:
                    tags.Add(new OverrideTag.I(args.Count > 0 ? args[0].ParseAssInt() == 1 : null));
                    break;
                case OverrideTags.IClip:
                    if (args.Count == 4)
                        tags.Add(
                            new OverrideTag.IClip(
                                args[0].ParseAssInt(),
                                args[1].ParseAssInt(),
                                args[2].ParseAssInt(),
                                args[3].ParseAssInt()
                            )
                        );
                    else
                        tags.Add(new OverrideTag.Unknown(OverrideTags.IClip, args.ToArray()));
                    break;
                case OverrideTags.K:
                    tags.Add(new OverrideTag.K(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.Kf:
                    tags.Add(new OverrideTag.Kf(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.Ko:
                    tags.Add(new OverrideTag.Ko(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.Kt:
                    tags.Add(new OverrideTag.Kt(args.Count > 0 ? args[0].ParseAssDouble() : null));
                    break;
                case OverrideTags.KUpper:
                    tags.Add(
                        new OverrideTag.KUpper(args.Count > 0 ? args[0].ParseAssDouble() : null)
                    );
                    break;
                case OverrideTags.Move:
                    if (args.Count == 4)
                        tags.Add(
                            new OverrideTag.Move(
                                args[0].ParseAssDouble(),
                                args[1].ParseAssDouble(),
                                args[2].ParseAssDouble(),
                                args[3].ParseAssDouble()
                            )
                        );
                    else if (args.Count == 6)
                        tags.Add(
                            new OverrideTag.Move(
                                args[0].ParseAssDouble(),
                                args[1].ParseAssDouble(),
                                args[2].ParseAssDouble(),
                                args[3].ParseAssDouble(),
                                args[4].ParseAssInt(),
                                args[5].ParseAssInt()
                            )
                        );
                    else
                        tags.Add(new OverrideTag.Unknown(OverrideTags.Move, args.ToArray()));
                    break;
                case OverrideTags.Org:
                    if (args.Count == 2)
                        tags.Add(
                            new OverrideTag.Org(args[0].ParseAssDouble(), args[1].ParseAssDouble())
                        );
                    else
                        tags.Add(new OverrideTag.Unknown(OverrideTags.Org, args.ToArray()));
                    break;
                case OverrideTags.P:
                    if (args.Count > 0)
                        tags.Add(new OverrideTag.P(args[0].ParseAssInt()));
                    else
                        tags.Add(new OverrideTag.Unknown(OverrideTags.P, args.ToArray()));
                    break;
                case OverrideTags.Pbo:
                    if (args.Count > 0)
                        tags.Add(new OverrideTag.Pbo(args[0].ParseAssDouble()));
                    else
                        tags.Add(new OverrideTag.Unknown(OverrideTags.Pbo, args.ToArray()));
                    break;
                case OverrideTags.Pos:
                    if (args.Count == 2)
                        tags.Add(
                            new OverrideTag.Pos(args[0].ParseAssDouble(), args[1].ParseAssDouble())
                        );
                    else
                        tags.Add(new OverrideTag.Unknown(OverrideTags.Pos, args.ToArray()));
                    break;
                case OverrideTags.Q:
                    tags.Add(new OverrideTag.Q(args.Count > 0 ? args[0].ParseAssInt() : null));
                    break;
                case OverrideTags.R:
                    tags.Add(new OverrideTag.R(args.Count > 0 ? args[0] : null));
                    break;
                case OverrideTags.S:
                    tags.Add(new OverrideTag.S(args.Count > 0 ? args[0].ParseAssInt() == 1 : null));
                    break;
                case OverrideTags.Shad:
                    tags.Add(
                        new OverrideTag.Shad(args.Count > 0 ? args[0].ParseAssDouble() : null)
                    );
                    break;
                case OverrideTags.T:
                {
                    var blockIdx = args.Count - 1;
                    if (blockIdx is < 0 or > 3 || !hasBackslashArg)
                    {
                        tags.Add(new OverrideTag.Unknown(OverrideTags.T, args.ToArray()));
                        break;
                    }

                    // Parse inner tags
                    var block = args[blockIdx].AsSpan();
                    var blockTags = ParseTags(block);

                    switch (blockIdx)
                    {
                        case 0: // \t(\xyz)
                            tags.Add(new OverrideTag.T(blockTags));
                            break;
                        case 1: // \t(1.0,\xyz)
                            tags.Add(new OverrideTag.T(args[0].ParseAssDouble(), blockTags));
                            break;
                        case 2: // \t(1,10,\xyz)
                            tags.Add(
                                new OverrideTag.T(
                                    args[0].ParseAssDouble(), // VSFilter moment or something, idk
                                    args[1].ParseAssDouble(),
                                    blockTags
                                )
                            );
                            break;
                        case 3: // \t(1,10,1.0,\xyz)
                            tags.Add(
                                new OverrideTag.T(
                                    args[0].ParseAssInt(),
                                    args[1].ParseAssInt(),
                                    args[2].ParseAssDouble(),
                                    blockTags
                                )
                            );
                            break;
                    }
                    break;
                }
                case OverrideTags.U:
                    tags.Add(new OverrideTag.U(args.Count > 0 ? args[0].ParseAssInt() == 1 : null));
                    break;
                case OverrideTags.XBord:
                    tags.Add(
                        new OverrideTag.XBord(args.Count > 0 ? args[0].ParseAssDouble() : null)
                    );
                    break;
                case OverrideTags.XShad:
                    tags.Add(
                        new OverrideTag.XShad(args.Count > 0 ? args[0].ParseAssDouble() : null)
                    );
                    break;
                case OverrideTags.YBord:
                    tags.Add(
                        new OverrideTag.YBord(args.Count > 0 ? args[0].ParseAssDouble() : null)
                    );
                    break;
                case OverrideTags.YShad:
                    tags.Add(
                        new OverrideTag.YShad(args.Count > 0 ? args[0].ParseAssDouble() : null)
                    );
                    break;
            }
        }
        return tags;
    }
}
