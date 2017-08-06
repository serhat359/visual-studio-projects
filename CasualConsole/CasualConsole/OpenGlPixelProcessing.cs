using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CasualConsole
{
    public class OpenGlPixelProcessing
    {
        static OpenGlPixelProcessing()
        {
            CudafyModes.Target = eGPUType.OpenCL; // To use OpenCL, change this enum
            CudafyModes.DeviceId = 0;
            CudafyTranslator.Language = CudafyModes.Target == eGPUType.OpenCL ? eLanguage.OpenCL : eLanguage.Cuda;
        }

        public static void TestMyMethod()
        {
            Bitmap bmp = GetBitMapFromFile(@"C:\Users\Xhertas\Pictures\harfler.png");

            ShowBmp(bmp);

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            MyExecute(rgbValues, bmp.Width, bmp.Height);

            // Copy the RGB values back to the bitmap
            Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            ShowBmp(bmp);
        }

        private static Bitmap GetBitMapFromFile(string filename)
        {
            Bitmap orig = new Bitmap(filename);
            Bitmap clone = new Bitmap(orig.Width, orig.Height, PixelFormat.Format32bppArgb);

            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(orig, new Rectangle(0, 0, clone.Width, clone.Height));
            }

            return clone;
        }

        public static void ShowBmp(Bitmap bmp)
        {
            var newwindow = new MyWindow(bmp, "Image");
            newwindow.Show();
            newwindow.Invalidate();

            Application.Run(newwindow);
        }

        public static void MyExecute(byte[] ptr, int dimX, int dimY)
        {
            CudafyModule km = CudafyTranslator.Cudafy();

            GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target, CudafyModes.DeviceId);
            gpu.LoadModule(km);

            byte[] allocated_dev_bitmap = gpu.Allocate<byte>(ptr.Length);

            byte[] copied_dev_bitmap = gpu.CopyToDevice(ptr);

            gpu.Launch(new dim3(dimX, dimY), 1).mykernel(allocated_dev_bitmap, copied_dev_bitmap);

            gpu.CopyFromDevice(allocated_dev_bitmap, ptr);

            gpu.FreeAll();
        }

        [Cudafy]
        public static void mykernel(GThread thread, byte[] dev_bitmap, byte[] original)
        {
            int x = thread.blockIdx.x;
            int y = thread.blockIdx.y;
            int pixelOffset = y * thread.gridDim.x + x;

            if (x >= 1 && x < thread.gridDim.x - 1)
            {
                if (y >= 1 && y < thread.gridDim.y - 1)
                {
                    int pixelByteOffset = pixelOffset * 4;

                    int leftPixelOffset = pixelByteOffset - 4;
                    int rightPixelOffset = pixelByteOffset + 4;
                    int abovePixelOffset = pixelByteOffset - thread.gridDim.x * 4;
                    int belowPixelOffset = pixelByteOffset + thread.gridDim.x * 4;

                    dev_bitmap[pixelByteOffset + 0] = (byte)((original[leftPixelOffset + 0] + original[rightPixelOffset + 0] + original[abovePixelOffset + 0] + original[belowPixelOffset + 0]) / 4);

                    dev_bitmap[pixelByteOffset + 1] = (byte)((original[leftPixelOffset + 1] + original[rightPixelOffset + 1] + original[abovePixelOffset + 1] + original[belowPixelOffset + 1]) / 4);

                    dev_bitmap[pixelByteOffset + 2] = (byte)((original[leftPixelOffset + 2] + original[rightPixelOffset + 2] + original[abovePixelOffset + 2] + original[belowPixelOffset + 2]) / 4);

                    dev_bitmap[pixelByteOffset + 3] = 255;
                }
            }
        }
    }
}
