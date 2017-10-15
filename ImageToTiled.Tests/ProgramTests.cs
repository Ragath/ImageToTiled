using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TiledLib.Layer;

namespace ImageToTiled.Tests
{
    [TestClass]
    public class ProgramTests
    {
        public TestContext TestContext { get; set; }
        
        [DataTestMethod]
        [DataRow("Data/map0.png", "Data/Overworld.tsx")]
        public void TestProcess(string image, string tileset)
        {
            var output = Program.GetOutputPath(Path.Combine(TestContext.TestRunResultsDirectory, Path.GetFileName(image)));

            var map = Program.Process(image, tileset, output);
            var layer = map.Layers.OfType<TileLayer>().Single();
            Assert.IsFalse(layer.Data.All(i => i == 0));

            using (var writer = XmlWriter.Create(output, new XmlWriterSettings { Encoding = Encoding.UTF8 }))
            {
                new XmlSerializer(typeof(TiledLib.Map)).Serialize(writer, map);
                TestContext.AddResultFile(output);
            }

            Assert.IsTrue(File.Exists(output));
            foreach (var ts in map.Tilesets.OfType<TiledLib.ExternalTileset>())
            {
                var path = Path.Combine(Path.GetDirectoryName(output), ts.source);
                Assert.IsTrue(File.Exists(path));
            }
            //Assert.Fail();
        }
    }
}
