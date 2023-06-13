using System.Drawing.Imaging;
using System.Reflection.Metadata;
using System.Text;

namespace MMSProject
{
    public partial class Form1 : Form
    {
        private Bitmap m_Bitmap;
        private Bitmap m_Undo;
        private double Zoom = 1.0;
        public Form1()
        {
            InitializeComponent();
            
            m_Bitmap = new Bitmap(2, 2);
        }
        private void SaveCustomImage(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                
                writer.Write(Encoding.ASCII.GetBytes("CIF"));
                writer.Write(m_Bitmap.Width);
                writer.Write(m_Bitmap.Height);
                writer.Write(Zoom);
                writer.Write(Downsampling.Checked);
                writer.Write(ShanonFano.Checked);

                //konvertovanje u YUV
                byte[,] yChannel, uChannel, vChannel;
                ConvertToYUV(m_Bitmap, out yChannel, out uChannel, out vChannel);

                // Downsampling ako je stiklirano
                bool applyDownsampling = Downsampling.Checked;
                if (applyDownsampling)
                {
                    uChannel = Downsample(uChannel);
                    vChannel = Downsample(vChannel);
                }

                // Kompresija ako je stiklirana trenutno ne radi
                bool applyCompression = ShanonFano.Checked;
                if (applyCompression)
                {
                    byte[] compressedY = CompressChannel(yChannel);
                    byte[] compressedU = CompressChannel(uChannel);
                    byte[] compressedV = CompressChannel(vChannel);

                    writer.Write(compressedY.Length);
                    writer.Write(compressedY);
                    writer.Write(compressedU.Length);
                    writer.Write(compressedU);
                    writer.Write(compressedV.Length);
                    writer.Write(compressedV);
                }
                else
                {
                    WriteChannelData(writer, yChannel);
                    WriteChannelData(writer, uChannel);
                    WriteChannelData(writer, vChannel);
                }
            }
        }
        private void ConvertToYUV(Bitmap bitmap, out byte[,] yChannel, out byte[,] uChannel, out byte[,] vChannel)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            yChannel = new byte[width, height];
            uChannel = new byte[width, height];
            vChannel = new byte[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    byte r = pixelColor.R;
                    byte g = pixelColor.G;
                    byte b = pixelColor.B;

                    byte yVal = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                    byte uVal = (byte)(-0.169 * r - 0.331 * g + 0.5 * b + 128);
                    byte vVal = (byte)(0.5 * r - 0.419 * g - 0.081 * b + 128);

                    yChannel[x, y] = yVal;
                    uChannel[x, y] = uVal;
                    vChannel[x, y] = vVal;
                }
            }
        }
        private byte[,] Downsample(byte[,] channel)
        {
            //4:2:0 metoda uzima uzimaju 2x2 piksela i pretvore se u jedan kasnije se upsamluje
            int width = channel.GetLength(0);
            int height = channel.GetLength(1);
            int newWidth = width / 2;
            int newHeight = height / 2;
            byte[,] downsampledChannel = new byte[newWidth, newHeight];

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int u = 2 * x;
                    int v = 2 * y;
                    int sum = channel[u, v] + channel[u + 1, v] + channel[u, v + 1] + channel[u + 1, v + 1];
                    downsampledChannel[x, y] = (byte)(sum / 4);
                }
            }

            return downsampledChannel;
        }
        private void WriteChannelData(BinaryWriter writer, byte[,] channel)
        {
            //helpper funkcija
            int width = channel.GetLength(0);
            int height = channel.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    writer.Write(channel[x, y]);
                }
            }
        }
        private byte[] CompressChannel(byte[,] channel)
        {
            int width = channel.GetLength(0);
            int height = channel.GetLength(1);
            List<byte> compressedData = new List<byte>();

            // pretvorimo iz 2d array u 1d
            byte[] flattenedChannel = new byte[width * height];
            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    flattenedChannel[index] = channel[x, y];
                    index++;
                }
            }

            // pokusaj Shanon Fano
            PerformShannonFanoCompression(flattenedChannel, 0, flattenedChannel.Length - 1, compressedData);

            return compressedData.ToArray();
        }
        private void PerformShannonFanoCompression(byte[] data, int start, int end, List<byte> compressedData)
        {
            if (start == end)
            {           
                compressedData.Add(data[start]);
            }
            else if (start < end)
            {
                int splitIndex = FindSplitIndex(data, start, end);

                for (int i = start; i <= splitIndex; i++)
                {
                    compressedData.Add(0); 
                    compressedData.Add(data[i]);
                }

                for (int i = splitIndex + 1; i <= end; i++)
                {
                    compressedData.Add(1); 
                    compressedData.Add(data[i]);
                }

                PerformShannonFanoCompression(data, start, splitIndex, compressedData);
                PerformShannonFanoCompression(data, splitIndex + 1, end, compressedData);
            }
        }
        private int FindSplitIndex(byte[] data, int start, int end)
        {
            //helpper funkcija
            int[] frequencies = new int[256];
            for (int i = start; i <= end; i++)
            {
                frequencies[data[i]]++;
            }

            int splitIndex = start;
            int sumLeft = 0;
            int sumRight = frequencies[start];
            int minDifference = Math.Abs(sumLeft - sumRight);

            for (int i = start + 1; i <= end; i++)
            {
                sumLeft += frequencies[i - 1];
                sumRight -= frequencies[i - 1];

                int difference = Math.Abs(sumLeft - sumRight);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    splitIndex = i;
                }
                else
                {
                    // Break the loop if the difference starts increasing
                    break;
                }
            }

            return splitIndex - 1;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.DrawImage(m_Bitmap, new Rectangle(this.AutoScrollPosition.X, this.AutoScrollPosition.Y, (int)(m_Bitmap.Width * Zoom), (int)(m_Bitmap.Height * Zoom)));
        }
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //loaduje sliku pa posle proverava da li je obicna ili moj custom format ako je custom ucita sliku posebno
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp|Jpeg files (*.jpg)|*.jpg|GIF files(*.gif)|*.gif|PNG files(*.png)|*.png|All valid files|*.bmp/*.jpg/*.gif/*.png|Custom Image Format (*.cif)|*.cif";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (DialogResult.OK == openFileDialog.ShowDialog())
            {
                string filePath = openFileDialog.FileName;

                if (Path.GetExtension(filePath).Equals(".cif", StringComparison.OrdinalIgnoreCase))
                {
                    LoadCustomImage(filePath);
                }
                else
                {
                    m_Bitmap = (Bitmap)Bitmap.FromFile(filePath, false);
                    this.AutoScroll = true;
                    this.AutoScrollMinSize = new Size((int)(m_Bitmap.Width * Zoom), (int)(m_Bitmap.Height * Zoom));
                    this.Invalidate();
                }
            }
        }
        private void pixelateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_Undo = (Bitmap)m_Bitmap.Clone();
            if (Pixelate(m_Bitmap, 15, false))
                this.Invalidate();
        }
        public static bool Pixelate(Bitmap b, short pixel, bool bGrid)
        {
            int nWidth = b.Width;
            int nHeight = b.Height;

            Point[,] pt = new Point[nWidth, nHeight];

            int newX, newY;

            for (int x = 0; x < nWidth; ++x)
                for (int y = 0; y < nHeight; ++y)
                {
                    newX = pixel - x % pixel;

                    if (bGrid && newX == pixel)
                        pt[x, y].X = -x;
                    else if (x + newX > 0 && x + newX < nWidth)
                        pt[x, y].X = newX;
                    else
                        pt[x, y].X = 0;

                    newY = pixel - y % pixel;

                    if (bGrid && newY == pixel)
                        pt[x, y].Y = -y;
                    else if (y + newY > 0 && y + newY < nHeight)
                        pt[x, y].Y = newY;
                    else
                        pt[x, y].Y = 0;
                }

            OffsetFilter(b, pt);

            return true;
        }
        public static bool OffsetFilter(Bitmap b, Point[,] offset)
        {
            Bitmap bSrc = (Bitmap)b.Clone();

            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int scanline = bmData.Stride;

            System.IntPtr Scan0 = bmData.Scan0;
            System.IntPtr SrcScan0 = bmSrc.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                byte* pSrc = (byte*)(void*)SrcScan0;

                int nOffset = bmData.Stride - b.Width * 3;
                int nWidth = b.Width;
                int nHeight = b.Height;

                int xOffset, yOffset;

                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        xOffset = offset[x, y].X;
                        yOffset = offset[x, y].Y;

                        if (y + yOffset >= 0 && y + yOffset < nHeight && x + xOffset >= 0 && x + xOffset < nWidth)
                        {
                            p[0] = pSrc[((y + yOffset) * scanline) + ((x + xOffset) * 3)];
                            p[1] = pSrc[((y + yOffset) * scanline) + ((x + xOffset) * 3) + 1];
                            p[2] = pSrc[((y + yOffset) * scanline) + ((x + xOffset) * 3) + 2];
                        }

                        p += 3;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);
            bSrc.UnlockBits(bmSrc);

            return true;
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //sacuva sliku u nas custom format 
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Custom Image Format (*.cif)|*.cif";
            saveFileDialog.DefaultExt = "cif";
            saveFileDialog.AddExtension = true;

            if (DialogResult.OK == saveFileDialog.ShowDialog())
            {
                string filePath = saveFileDialog.FileName;
                SaveCustomImage(filePath);
            }
        }
        private void LoadCustomImage(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                byte[] magicNumberBytes = reader.ReadBytes(3);
                string magicNumber = Encoding.ASCII.GetString(magicNumberBytes);
                if (magicNumber != "CIF")
                {
                    MessageBox.Show("Invalid CIF file.");
                    return;
                }

                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                double zoom = reader.ReadDouble();
                bool Downsampled = reader.ReadBoolean();
                bool ShanonFanoned = reader.ReadBoolean();

                byte[,] yChannel, uChannel, vChannel;


                if (ShanonFanoned)
                {
                    int compressedYLength = reader.ReadInt32();
                    byte[] compressedY = reader.ReadBytes(compressedYLength);
                    int compressedULength = reader.ReadInt32();
                    byte[] compressedU = reader.ReadBytes(compressedULength);
                    int compressedVLength = reader.ReadInt32();
                    byte[] compressedV = reader.ReadBytes(compressedVLength);

                    yChannel = DecompressChannel(compressedY, width, height);
                    uChannel = DecompressChannel(compressedY, width, height);
                    vChannel = DecompressChannel(compressedY, width, height);

                }
                else
                {
                    yChannel = ReadChannelData(reader, width, height);
                    if (Downsampled)
                    {
                        uChannel = ReadChannelDataReconstruct(reader, width, height);
                        vChannel = ReadChannelDataReconstruct(reader, width, height);
                    }
                    else
                    {
                        uChannel = ReadChannelData(reader, width, height);
                        vChannel = ReadChannelData(reader, width, height);
                    }


                }

                Bitmap bitmap = ConvertToRGB(width, height, yChannel, uChannel, vChannel);


                int zoomedWidth = (int)(width * zoom);
                int zoomedHeight = (int)(height * zoom);
                bitmap = new Bitmap(bitmap, zoomedWidth, zoomedHeight);

                m_Bitmap = bitmap;
                this.AutoScrollMinSize = new Size(zoomedWidth, zoomedHeight);
                this.Invalidate();
            }
        }
        private byte[,] DecompressChannel(byte[] compressedData, int width, int height)
        {
            byte[,] channel = new byte[width, height];

            List<byte> compressedList = new List<byte>(compressedData);

            PerformShannonFanoDecompression(channel, compressedList, 0, width * height - 1, 0);

            return channel;
        }
        private void PerformShannonFanoDecompression(byte[,] channel, List<byte> compressedData, int start, int end, int currentIndex)
        {
            if (start == end)
            {
                channel[currentIndex % channel.GetLength(0), currentIndex / channel.GetLength(0)] = compressedData[0];
                compressedData.RemoveAt(0);
            }
            else if (start < end)
            {
                byte prefix = compressedData[0];
                compressedData.RemoveAt(0);

                int splitIndex = DecodeSymbols(channel, compressedData, start, end, currentIndex, prefix);

                PerformShannonFanoDecompression(channel, compressedData, start, splitIndex, currentIndex * 2);
                PerformShannonFanoDecompression(channel, compressedData, splitIndex + 1, end, currentIndex * 2 + 1);
            }
        }
        private int DecodeSymbols(byte[,] channel, List<byte> compressedData, int start, int end, int currentIndex, byte prefix)
        {
            int splitIndex = start - 1;

            for (int i = start; i <= end; i++)
            {
                if (compressedData.Count == 0)
                {
                    break;
                }

                if (compressedData[0] == prefix)
                {
                    splitIndex = i;
                    compressedData.RemoveAt(0);
                    channel[currentIndex % channel.GetLength(0), currentIndex / channel.GetLength(0)] = compressedData[0];
                    compressedData.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            return splitIndex;
        }
        private byte[,] ReadChannelData(BinaryReader reader, int width, int height)
        {
            byte[,] channel = new byte[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    channel[x, y] = reader.ReadByte();
                }
            }

            return channel;
        }
        private byte[,] ReadChannelDataReconstruct(BinaryReader reader, int width, int height)
        {
            byte[,] channel = new byte[width, height];

            for (int y = 0; y < height; y+=2)
            {
                for (int x = 0; x < width; x+=2)
                {
                    if (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        byte data = reader.ReadByte();
                       
                        if (x < width - 1 && y < height - 1)
                        {
                            channel[x, y] = data;
                            channel[x, y + 1] = data;
                            channel[x + 1, y] = data;
                            channel[x + 1, y + 1] = data;
                        }
                        else if (x == width - 1 && y < height - 1)
                        {
                            channel[x, y] = data;
                            channel[x, y + 1] = data;
                        }
                        else if (x < width - 1 && y == height - 1)
                        {
                            channel[x, y] = data;
                            channel[x + 1, y] = data;
                        }
                        else if (x == width - 1 && y == height - 1)
                        {

                            channel[x, y] = data;
                        }
                    }
                }
            }

            return channel;
        }
        private Bitmap ConvertToRGB(int width, int height, byte[,] yChannel, byte[,] uChannel, byte[,] vChannel)
        {
            Bitmap bitmap = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte yVal = yChannel[x, y];
                    byte uVal = uChannel[x, y];
                    byte vVal = vChannel[x, y];

                    int r = (int)(yVal + 1.402 * (vVal - 128));
                    int g = (int)(yVal - 0.344136 * (uVal - 128) - 0.714136 * (vVal - 128));
                    int b = (int)(yVal + 1.772 * (uVal - 128));

                    r = Math.Max(0, Math.Min(255, r));
                    g = Math.Max(0, Math.Min(255, g));
                    b = Math.Max(0, Math.Min(255, b));

                    Color color = Color.FromArgb(r, g, b);
                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;
        }
        private void edgeHomogenityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EdgeParamter dlg = new EdgeParamter();
            dlg.nValue = 0;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                m_Undo = (Bitmap)m_Bitmap.Clone();
                if (EdgeDetectHomogenity(m_Bitmap, (byte)dlg.nValue))
                    this.Invalidate();
            }
        }
        public static bool EdgeDetectHomogenity(Bitmap b, byte nThreshold)
        {
           
            Bitmap b2 = (Bitmap)b.Clone();

            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmData2 = b2.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            System.IntPtr Scan02 = bmData2.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                byte* p2 = (byte*)(void*)Scan02;

                int nOffset = stride - b.Width * 3;
                int nWidth = b.Width * 3;

                int nPixel = 0, nPixelMax = 0;

                p += stride;
                p2 += stride;

                for (int y = 1; y < b.Height - 1; ++y)
                {
                    p += 3;
                    p2 += 3;

                    for (int x = 3; x < nWidth - 3; ++x)
                    {
                        nPixelMax = Math.Abs(p2[0] - (p2 + stride - 3)[0]);
                        nPixel = Math.Abs(p2[0] - (p2 + stride)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 + stride + 3)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 - stride)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 + stride)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 - stride - 3)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 - stride)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 - stride + 3)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        if (nPixelMax < nThreshold) nPixelMax = 0;

                        p[0] = (byte)nPixelMax;

                        ++p;
                        ++p2;
                    }

                    p += 3 + nOffset;
                    p2 += 3 + nOffset;
                }
            }

            b.UnlockBits(bmData);
            b2.UnlockBits(bmData2);

            return true;

        }
        private void gamaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Gama dlg = new Gama();
            dlg.red = dlg.green = dlg.blue = 1;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                m_Undo = (Bitmap)m_Bitmap.Clone();
                if (Gamma(m_Bitmap, dlg.red, dlg.green, dlg.blue))
                    this.Invalidate();
            }
        }
        public static bool Gamma(Bitmap b, double red, double green, double blue)
        {
            if (red < .2 || red > 5) return false;
            if (green < .2 || green > 5) return false;
            if (blue < .2 || blue > 5) return false;

            byte[] redGamma = new byte[256];
            byte[] greenGamma = new byte[256];
            byte[] blueGamma = new byte[256];

            for (int i = 0; i < 256; ++i)
            {
                redGamma[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / red)) + 0.5));
                greenGamma[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / green)) + 0.5));
                blueGamma[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / blue)) + 0.5));
            }

           
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - b.Width * 3;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        p[2] = redGamma[p[2]];
                        p[1] = greenGamma[p[1]];
                        p[0] = blueGamma[p[0]];

                        p += 3;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);

            return true;
        }
        private void embossLaplacianToolStripMenuItem_Click(object sender, EventArgs e)
        {
                m_Undo = (Bitmap)m_Bitmap.Clone();
                if (EmbossLaplacian(m_Bitmap))
                    this.Invalidate();
        }
        public class ConvMatrix
        {
            public int TopLeft = 0, TopMid = 0, TopRight = 0;
            public int MidLeft = 0, Pixel = 1, MidRight = 0;
            public int BottomLeft = 0, BottomMid = 0, BottomRight = 0;
            public int Factor = 1;
            public int Offset = 0;
            public void SetAll(int nVal)
            {
                TopLeft = TopMid = TopRight = MidLeft = Pixel = MidRight = BottomLeft = BottomMid = BottomRight = nVal;
            }
        }
        public static bool EmbossLaplacian(Bitmap b)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(-1);
            m.TopMid = m.MidLeft = m.MidRight = m.BottomMid = 0;
            m.Pixel = 4;
            m.Offset = 127;

            return Conv3x3(b, m);
        }
        public static bool Conv3x3(Bitmap b, ConvMatrix m)
        {
            if (0 == m.Factor) return false;

            Bitmap bSrc = (Bitmap)b.Clone();

            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            int stride2 = stride * 2;
            System.IntPtr Scan0 = bmData.Scan0;
            System.IntPtr SrcScan0 = bmSrc.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                byte* pSrc = (byte*)(void*)SrcScan0;

                int nOffset = stride - b.Width * 3;
                int nWidth = b.Width - 2;
                int nHeight = b.Height - 2;

                int nPixel;

                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        nPixel = ((((pSrc[2] * m.TopLeft) + (pSrc[5] * m.TopMid) + (pSrc[8] * m.TopRight) +
                            (pSrc[2 + stride] * m.MidLeft) + (pSrc[5 + stride] * m.Pixel) + (pSrc[8 + stride] * m.MidRight) +
                            (pSrc[2 + stride2] * m.BottomLeft) + (pSrc[5 + stride2] * m.BottomMid) + (pSrc[8 + stride2] * m.BottomRight)) / m.Factor) + m.Offset);

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;

                        p[5 + stride] = (byte)nPixel;

                        nPixel = ((((pSrc[1] * m.TopLeft) + (pSrc[4] * m.TopMid) + (pSrc[7] * m.TopRight) +
                            (pSrc[1 + stride] * m.MidLeft) + (pSrc[4 + stride] * m.Pixel) + (pSrc[7 + stride] * m.MidRight) +
                            (pSrc[1 + stride2] * m.BottomLeft) + (pSrc[4 + stride2] * m.BottomMid) + (pSrc[7 + stride2] * m.BottomRight)) / m.Factor) + m.Offset);

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;

                        p[4 + stride] = (byte)nPixel;

                        nPixel = ((((pSrc[0] * m.TopLeft) + (pSrc[3] * m.TopMid) + (pSrc[6] * m.TopRight) +
                            (pSrc[0 + stride] * m.MidLeft) + (pSrc[3 + stride] * m.Pixel) + (pSrc[6 + stride] * m.MidRight) +
                            (pSrc[0 + stride2] * m.BottomLeft) + (pSrc[3 + stride2] * m.BottomMid) + (pSrc[6 + stride2] * m.BottomRight)) / m.Factor) + m.Offset);

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;

                        p[3 + stride] = (byte)nPixel;

                        p += 3;
                        pSrc += 3;
                    }
                    p += nOffset;
                    pSrc += nOffset;
                }
            }

            b.UnlockBits(bmData);
            bSrc.UnlockBits(bmSrc);

            return true;
        }
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap temp = (Bitmap)m_Bitmap.Clone();
            m_Bitmap = (Bitmap)m_Undo.Clone();
            m_Undo = (Bitmap)temp.Clone();
            this.Invalidate();
        }
    }
}