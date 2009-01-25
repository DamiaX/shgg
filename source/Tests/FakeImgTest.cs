/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
*/

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using NUnit.Framework;
using HAKGERSoft;

namespace HAKGERSoft.Tests {

    [TestFixture]
    public class FakeImgTest {

        [Test]
        public void ImgRequestTest() {
            ConnectionMock mc = new ConnectionMock();
            sHGG ggmock = new sHGG(mc);
            string msg = "Simple message with image inside";
            int rec = 956267;
            int imgPos = 4;
            MemoryStream stream = new MemoryStream();
            using (Bitmap img = CreateRandomImage(600, 200)) {
                img.Save(stream, ImageFormat.Jpeg);
            }

            ggmock.GGSendImage(rec, msg, imgPos, stream);

            // msg
            Assert.AreEqual(mc.data.Length, 36 + msg.Length + 1);
            Assert.AreEqual(mc.ReadUInt(), 0xb); // type
            Assert.AreEqual(mc.ReadUInt(), 28 + msg.Length + 1); // size 
            Assert.AreEqual(mc.ReadUInt(), rec); // rec
            mc.ReadUInt(); // seq
            mc.ReadUInt(); // msg class
            for (int i = 0;i < msg.Length;i++)
                Assert.AreEqual(mc.ReadByte(), Convert.ToByte(msg[i]));
            Assert.AreEqual(mc.ReadByte(), 0); // null char
            Assert.IsFalse(mc.IsEnd);
            // rich info
            Assert.AreEqual(mc.ReadByte(), 2); // rich info flag
            Assert.AreEqual(mc.ReadShort(), 13); // rich length
            // rich format list
            Assert.AreEqual(mc.ReadShort(), imgPos); // pos
            Assert.AreEqual(mc.ReadByte(), 0x0 | 0x80); // font (image)
            Assert.IsFalse(mc.IsEnd);
            // image
            Assert.AreEqual(mc.ReadShort(), 0x109); // unknown flag
            Assert.AreEqual(mc.ReadUInt(), stream.Length);
            Assert.AreEqual(mc.ReadUInt(), new CRC32().GetCrc32(stream));
            Assert.IsTrue(mc.IsEnd);         
        }

        [Test]
        [Ignore]
        public void ImgReplySmallTest() {
            ConnectionMock mc = new ConnectionMock();
            sHGG ggmock = new sHGG(mc);
            uint rec = 956267;
            MemoryStream stream = new MemoryStream();
            using (Bitmap img = CreateRandomImage(100, 200)) {
                img.Save(stream, ImageFormat.Jpeg);
            }
            // hack
            ggmock.imageBuff.pushSave(956267, new sHGG.imageBuffEl() {
                bin = sHGG.Stream2Array(stream),
                path = "file.jpg"
            });

            ggmock.ImageReply((int)stream.Length, new CRC32().GetCrc32(stream), rec);

            // todo

        }

        [Test]
        [Ignore]
        public void ImgReplyBigTest() {
            // todo

        }

        private Bitmap CreateRandomImage(int width, int height) {
            string p = "FD6GFGHF5466M4ROP5BA2456KLAS3E2354ROP5BA2456SDG";
            Random r = new Random();
            Bitmap b = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(b);
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.Clear(Color.Blue);
            Matrix m = new Matrix();
            for (int i = 0;i < p.Length;i++) {
                m.Reset();
                m.RotateAt(r.Next(-30, 30), new PointF(Convert.ToInt64(width * (0.10 * i)), Convert.ToInt64(height * 0.5)));
                g.Transform = m;
                Font font = new Font("Verdana", 30);
                g.DrawString(p[i].ToString(), font, SystemBrushes.ActiveCaptionText, Convert.ToInt64(width * (0.10 * i)), Convert.ToInt64(height * 0.1));
                g.ResetTransform();
            }
            g.Dispose();
            return b;
        }



    }
}
