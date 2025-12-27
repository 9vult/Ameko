// SPDX-License-Identifier: MPL-2.0

using AssCS.Overrides;
using AssCS.Overrides.Blocks;

namespace AssCS.Tests;

public class OverrideBlockTests
{
    #region Boolean Tags

    [Test]
    public async Task ItalicEnabled()
    {
        const string body = @"\i1";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var tag = block.Tags[0];

        var i = await Assert.That(tag).IsTypeOf<OverrideTag.I>();
        await Assert.That(i).IsNotNull();

        await Assert.That(i.Value).IsTrue();
        await Assert.That(i.ToString()).IsEqualTo(body);
    }

    [Test]
    public async Task ItalicDisabled()
    {
        const string body = @"\i0";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var tag = block.Tags[0];

        var i = await Assert.That(tag).IsTypeOf<OverrideTag.I>();
        await Assert.That(i).IsNotNull();

        await Assert.That(i.Value).IsFalse();
        await Assert.That(i.ToString()).IsEqualTo(body);
    }

    [Test]
    public async Task ItalicEmpty()
    {
        const string body = @"\i";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var tag = block.Tags[0];

        var i = await Assert.That(tag).IsTypeOf<OverrideTag.I>();
        await Assert.That(i).IsNotNull();

        await Assert.That(i.Value).IsNull();
        await Assert.That(i.ToString()).IsEqualTo(body);
    }

    [Test]
    public async Task ItalicWithParentheses()
    {
        const string body = @"\i(1)";
        const string sanitized = @"\i1";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var tag = block.Tags[0];

        var i = await Assert.That(tag).IsTypeOf<OverrideTag.I>();
        await Assert.That(i).IsNotNull();

        await Assert.That(i.Value).IsTrue();
        await Assert.That(i.ToString()).IsEqualTo(sanitized);
    }

    #endregion Boolean Tags

    #region Font Size

    [Test]
    public async Task FontSize_Integer()
    {
        const string body = @"\fs72";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var fs = await Assert.That(block.Tags[0]).IsTypeOf<OverrideTag.Fs>();

        await Assert.That(fs).IsNotNull();
        await Assert.That(fs.Variant).IsEqualTo(OverrideTag.Fs.FsVariant.Absolute);
        await Assert.That(fs.Value).IsEqualTo(72);

        await Assert.That(fs.ToString()).IsEqualTo(body);
    }

    [Test]
    public async Task FontSize_Decimal()
    {
        const string body = @"\fs72.5";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var fs = await Assert.That(block.Tags[0]).IsTypeOf<OverrideTag.Fs>();

        await Assert.That(fs).IsNotNull();
        await Assert.That(fs.Variant).IsEqualTo(OverrideTag.Fs.FsVariant.Absolute);
        await Assert.That(fs.Value).IsEqualTo(72.5);

        await Assert.That(fs.ToString()).IsEqualTo(body);
    }

    [Test]
    public async Task FontSize_RelativeIncrease()
    {
        const string body = @"\fs+10";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var fs = await Assert.That(block.Tags[0]).IsTypeOf<OverrideTag.Fs>();

        await Assert.That(fs).IsNotNull();
        await Assert.That(fs.Variant).IsEqualTo(OverrideTag.Fs.FsVariant.Relative);
        await Assert.That(fs.Value).IsEqualTo(10);

        await Assert.That(fs.ToString()).IsEqualTo(body);
    }

    [Test]
    public async Task FontSize_RelativeDecrease()
    {
        const string body = @"\fs-10";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var fs = await Assert.That(block.Tags[0]).IsTypeOf<OverrideTag.Fs>();

        await Assert.That(fs).IsNotNull();
        await Assert.That(fs.Variant).IsEqualTo(OverrideTag.Fs.FsVariant.Relative);
        await Assert.That(fs.Value).IsEqualTo(-10);

        await Assert.That(fs.ToString()).IsEqualTo(body);
    }

    #endregion Font Size

    #region Transform

    [Test]
    public async Task Transform_1Param()
    {
        const string body = @"\t(\fscx120\fscy400)";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var t = await Assert.That(block.Tags[0]).IsTypeOf<OverrideTag.T>();

        await Assert.That(t).IsNotNull();
        await Assert.That(t.Variant).IsEqualTo(OverrideTag.T.TransformVariant.BlockOnly);

        await Assert.That(t.Block.Count).IsEqualTo(2);

        var fscx = await Assert.That(t.Block[0]).IsTypeOf<OverrideTag.FscX>();
        var fscy = await Assert.That(t.Block[1]).IsTypeOf<OverrideTag.FscY>();

        await Assert.That(fscx!.Value).IsEqualTo(120);
        await Assert.That(fscy!.Value).IsEqualTo(400);
    }

    [Test]
    public async Task Transform_2Param()
    {
        const string body = @"\t(1.5,\fscx120\fscy400)";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var t = await Assert.That(block.Tags[0]).IsTypeOf<OverrideTag.T>();

        await Assert.That(t).IsNotNull();
        await Assert.That(t.Variant).IsEqualTo(OverrideTag.T.TransformVariant.AccelerationOnly);

        await Assert.That(t.Acceleration).IsEqualTo(1.5);
        await Assert.That(t.Block.Count).IsEqualTo(2);

        var fscx = await Assert.That(t.Block[0]).IsTypeOf<OverrideTag.FscX>();
        var fscy = await Assert.That(t.Block[1]).IsTypeOf<OverrideTag.FscY>();

        await Assert.That(fscx!.Value).IsEqualTo(120);
        await Assert.That(fscy!.Value).IsEqualTo(400);
    }

    [Test]
    public async Task Transform_3Param()
    {
        const string body = @"\t(10,40,\fscx120\fscy400)";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var t = await Assert.That(block.Tags[0]).IsTypeOf<OverrideTag.T>();

        await Assert.That(t).IsNotNull();
        await Assert.That(t.Variant).IsEqualTo(OverrideTag.T.TransformVariant.TimeOnly);

        await Assert.That(t.T1).IsEqualTo(10);
        await Assert.That(t.T2).IsEqualTo(40);
        await Assert.That(t.Block.Count).IsEqualTo(2);

        var fscx = await Assert.That(t.Block[0]).IsTypeOf<OverrideTag.FscX>();
        var fscy = await Assert.That(t.Block[1]).IsTypeOf<OverrideTag.FscY>();

        await Assert.That(fscx!.Value).IsEqualTo(120);
        await Assert.That(fscy!.Value).IsEqualTo(400);
    }

    [Test]
    public async Task Transform_4Param()
    {
        const string body = @"\t(10,40,1.5,\fscx120\fscy400)";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var t = await Assert.That(block.Tags[0]).IsTypeOf<OverrideTag.T>();

        await Assert.That(t).IsNotNull();
        await Assert.That(t.Variant).IsEqualTo(OverrideTag.T.TransformVariant.Full);

        await Assert.That(t.T1).IsEqualTo(10);
        await Assert.That(t.T2).IsEqualTo(40);
        await Assert.That(t.Acceleration).IsEqualTo(1.5);
        await Assert.That(t.Block.Count).IsEqualTo(2);

        var fscx = await Assert.That(t.Block[0]).IsTypeOf<OverrideTag.FscX>();
        var fscy = await Assert.That(t.Block[1]).IsTypeOf<OverrideTag.FscY>();

        await Assert.That(fscx!.Value).IsEqualTo(120);
        await Assert.That(fscy!.Value).IsEqualTo(400);
    }

    #endregion Transform

    [Test]
    public async Task InlineCodeAndVariables()
    {
        const string body = @"\t($sstart,!$sstart+$sdur*0.3!,\c!gc(2)!)";
        var block = new OverrideBlock(body);

        await Assert.That(block.Tags.Count).IsEqualTo(1);
        var t = await Assert.That(block.Tags[0]).IsTypeOf<OverrideTag.T>();

        await Assert.That(t).IsNotNull();
        await Assert.That(t.RawT1).IsEqualTo("$sstart");
        await Assert.That(t.RawT2).IsEqualTo("!$sstart+$sdur*0.3!");

        await Assert.That(t.Block.Count).IsEqualTo(1);
        var c = await Assert.That(t.Block[0]).IsTypeOf<OverrideTag.C>();

        await Assert.That(c).IsNotNull();
        await Assert.That(c.Value).IsEqualTo("!gc(2)!");

        await Assert.That(t.ToString()).IsEqualTo(body);
    }
}
