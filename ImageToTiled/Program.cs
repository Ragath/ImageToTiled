using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TiledLib;
using TiledLib.Layer;

namespace ImageToTiled
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 3)
                Console.WriteLine("ImageToTiled.exe <tileset *.tsx> <image *.png> <output?>");

            var ts = args.ElementAtOrDefault(1)?.Trim('"')?.Trim() ?? GetPath("Tileset", ".tsx", ".json");
            var img = args.ElementAtOrDefault(0)?.Trim('"')?.Trim() ?? GetPath("Image", ".png", ".bmp", ".gif");
            var output = args.ElementAtOrDefault(1)?.Trim()?.Trim('"') ?? GetOutputPath(img);

            var map = Process(img, ts, output);

            using (var stream = File.Create(output))
            {
                new XmlSerializer(typeof(Map)).Serialize(stream, map);
            }

            Console.WriteLine("Output created at: " + Path.GetFullPath(output));
        }

        public static Map Process(string imagePath, string tilesetPath, string output)
        {
            Tileset ts;
            var dict = new Dictionary<byte[], int>(new ByteArrayComparer());
            using (var stream = File.OpenRead(tilesetPath))
            {
                ts = Tileset.FromStream(stream);
                if (ts.FirstGid == 0)
                    ts.FirstGid = 1;

                var imgPath = Path.Combine(Path.GetDirectoryName(tilesetPath), ts.ImagePath);
                using (var tsImg = new Bitmap(imgPath))
                {
                    for (int gid = ts.TileCount + ts.FirstGid - 1; gid >= ts.FirstGid; gid--)
                        dict[tsImg.GetTileHash(ts[gid].GetRectangle())] = gid;
                }
            }
            TileLayer layer;
            using (var img = new Bitmap(imagePath))
            {
                var tw = ts.TileWidth;
                var th = ts.TileHeight;

                var columns = img.Width / tw;
                var rows = img.Height / th;
                layer = new TileLayer()
                {
                    Name = "Layer0",
                    Encoding = "csv",
                    LayerType = LayerType.tilelayer,
                    X = 0,
                    Y = 0,
                    Width = columns,
                    Height = rows,
                    Opacity = 1,
                    Visible = true,
                    Data = new int[columns * rows]
                };


                for (int r = 0, i = 0; r < rows; r++)
                    for (int c = 0; c < columns; c++, i++)
                    {
                        var rect = new Rectangle(c * tw, r * th, tw, th);
                        var hash = img.GetTileHash(rect);
                        layer.Data[i] = dict.TryGetValue(hash, out var gid) ? gid : 0;
                    }
            }
            var source = tilesetPath.MakeRelativePath(output).Replace('\\', '/');
            var map = new Map()
            {
                CellWidth = ts.TileWidth,
                CellHeight = ts.TileHeight,
                Width = layer.Width,
                Height = layer.Height,
                Orientation = Orientation.orthogonal,
                RenderOrder = RenderOrder.rightdown,
                Tilesets = new ITileset[] { new ExternalTileset { FirstGid = ts.FirstGid, source = source } },
                Layers = new BaseLayer[] { layer }
            };

            return map;
        }

        static string GetPath(string name, params string[] expectedExtensions)
        {
            bool Validate(string path)
            {
                if (File.Exists(path))
                {
                    var ext = Path.GetExtension(path);
                    if (ext != "" && !expectedExtensions.Any(e => ext == e))
                        Console.WriteLine($"Warning, unknown extension: *{ext}");

                    return true;
                }
                else
                {
                    Console.WriteLine($"Error: {path} does not exist.");
                    return false;
                }
            }

            string result;
            do
            {
                Console.Write(name + ": ");
                result = Console.ReadLine().Trim('"').Trim();
            }
            while (!Validate(result));

            return result;
        }

        public static string GetOutputPath(string inputPath)
              => Path.GetExtension(inputPath) == ".tmx" ? Path.ChangeExtension(inputPath, null) + "_output.tmx" : Path.ChangeExtension(inputPath, "tmx");
    }

    static class Extensions
    {
        public static string MakeRelativePath(this string path, string relativeTo)
              => new Uri(Path.GetDirectoryName(Path.GetFullPath(relativeTo)) + Path.DirectorySeparatorChar)
              .MakeRelativeUri(new Uri(Path.GetFullPath(path)))
              .ToString();
    }
}
