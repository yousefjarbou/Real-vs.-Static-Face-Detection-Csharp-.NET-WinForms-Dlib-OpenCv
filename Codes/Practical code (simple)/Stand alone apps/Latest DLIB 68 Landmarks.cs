using DlibDotNet;
using DlibDotNet.Extensions;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using Point = System.Drawing.Point;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics; // For the Stopwatch


namespace DlibBlinkDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            var shapePredictor = ShapePredictor.Deserialize(@"C:\Users\Yousef.J\source\repos\realVSstaticFaceDetection\realVSstaticFaceDetection\shape_predictor_68_face_landmarks.dat");
            var faceDetector = Dlib.GetFrontalFaceDetector();
            var videoCapture = new VideoCapture(0, VideoCapture.API.DShow);

            double differenceThreshold = 0.45;
            int blinkCount = 0;
            bool eyesClosed = false;
            int consecutiveFrames = 0;
            bool timerActive = false;
            Timer testTimer = null;
            double difference = 0;
            Stopwatch stopwatch = new Stopwatch();
            List<double> earReadings = new List<double>();

            while (true)
            {
                using (var frame = new Mat())
                {
                    videoCapture.Read(frame);

                    // Convert Mat to Bitmap
                    Bitmap bitmap = MatToBitmap(frame);

                    // Convert Bitmap to Dlib Array2D<RgbPixel>
                    var dlibImage = BitmapExtensions.ToArray2D<RgbPixel>(bitmap);

                    // Detect faces
                    var faces = faceDetector.Operator(dlibImage);
                    if (faces.Length == 0)
                    {
                        blinkCount = 0;
                        earReadings.Clear();
                    }
                    foreach (var face in faces)
                    {
                        var shape = shapePredictor.Detect(dlibImage, face);
                        var leftEAR = CalculateEAR(shape, 36, 37, 38, 41, 40, 39);
                        var rightEAR = CalculateEAR(shape, 42, 43, 44, 47, 46, 45);

                        double ear = (leftEAR + rightEAR) / 2.0;
                        //CvInvoke.PutText(frame, ear.ToString("F2"), new Point(50, 160), FontFace.HersheySimplex, 1.0, new MCvScalar(255, 0, 0), 2);

                        earReadings.Add(ear);

                        if (earReadings.Count == 20)
                        {

                            earReadings.Sort();

                            double min1 = earReadings[0];
                            double min2 = earReadings[1];
                            double max1 = earReadings[19];
                            double max2 = earReadings[18];

                            // Calculate the difference
                            difference = (max1 + max2) - (min1 + min2);
                            if (difference > differenceThreshold)
                            {
                                blinkCount++;
                            }
                            earReadings.Clear();
                        }



                        // Draw landmarks and face bounding box
                        for (uint i = 0; i < shape.Parts; i++) // Use 'uint' for indexing
                        {
                            var point = new Point(shape.GetPart(i).X, shape.GetPart(i).Y);
                            CvInvoke.Circle(frame, point, 2, new MCvScalar(0, 255, 0), -1);
                        }

                        CvInvoke.Rectangle(frame, new System.Drawing.Rectangle(face.Left, face.Top, (int)face.Width, (int)face.Height), new MCvScalar(0, 255, 0), 2);

                        // Display blink count
                        CvInvoke.PutText(frame, $"Blinks: {blinkCount}", new Point(10, 60), FontFace.HersheySimplex, 1.0, new MCvScalar(255, 0, 0), 2);

                    }

                    // Display test results if the timer is active and has finished
                    if (timerActive)
                    {
                        CvInvoke.PutText(frame, $"{15 - (int)stopwatch.Elapsed.TotalSeconds} sec for the result", new Point(200, 60), FontFace.HersheySimplex, 1.1, new MCvScalar(255, 0, 0), 2);
                    }

                    //CvInvoke.Imshow("Blink Detection with Dlib", frame);
                    int frameWidth = frame.Width;
                    int frameHeight = frame.Height;

                    //CvInvoke.Resize(frame, frame, new Size(frameWidth, frameHeight));

                    Mat bottomRow = new Mat(new Size(frameWidth, 100), DepthType.Cv8U, 3);
                    bottomRow.SetTo(new MCvScalar(255, 255, 255)); // Set to white

                    // Combine the frame and the bottom row into a single Mat using VConcat
                    Mat combinedFrame = new Mat();
                    CvInvoke.VConcat(frame, bottomRow, combinedFrame);




                    //print in the row down
                    if (!timerActive)
                    {
                        CvInvoke.PutText(combinedFrame, "Press S to start", new Point(150, 550), FontFace.HersheySimplex, 1.2, new MCvScalar(0, 0, 0), 2);
                    }
                    if (timerActive)
                    {
                        if (blinkCount < 4) CvInvoke.PutText(combinedFrame, "Static Picture", new Point(frame.Width / 2 - 180, 550), FontFace.HersheySimplex, 1.8, new MCvScalar(0, 0, 255), 2);
                        else
                            CvInvoke.PutText(combinedFrame, "Real Person", new Point(frame.Width / 2 - 180, 550), FontFace.HersheySimplex, 1.8, new MCvScalar(255, 0, 0), 2);
                    }



                    // Display the combined frame
                    CvInvoke.Imshow("Blink Detection with Info Row", combinedFrame);


                    int key = CvInvoke.WaitKey(1);
                    if (key == 27) // Escape key
                    {
                        break;
                    }
                    else if (key == 's' || key == 'S')
                    {
                        blinkCount = 0;
                        timerActive = true;
                        stopwatch.Restart(); // Start the stopwatch to track elapsed time

                        // Start a new timer for 15 seconds
                        testTimer = new Timer(_ =>
                        {
                            timerActive = false;
                        }, null, 15000, Timeout.Infinite);
                    }
                }
            }
        }

        public static Bitmap MatToBitmap(Mat mat)
        {
            Bitmap bitmap = new Bitmap(mat.Width, mat.Height, PixelFormat.Format24bppRgb);

            BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                    ImageLockMode.WriteOnly, bitmap.PixelFormat);

            int byteCount = mat.Width * mat.Height * mat.ElementSize;
            byte[] data = new byte[byteCount];

            Marshal.Copy(mat.DataPointer, data, 0, byteCount);

            Marshal.Copy(data, 0, bitmapData.Scan0, byteCount);

            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        // Calculate Eye Aspect Ratio (EAR)
        static double CalculateEAR(FullObjectDetection shape, int p1, int p2, int p3, int p4, int p5, int p6)
        {
            double a = Distance(shape.GetPart((uint)p2), shape.GetPart((uint)p6));
            double b = Distance(shape.GetPart((uint)p3), shape.GetPart((uint)p5));
            double c = Distance(shape.GetPart((uint)p1), shape.GetPart((uint)p4));

            return (a + b) / (2.0 * c);
        }

        // Calculate Euclidean distance between two points
        static double Distance(DlibDotNet.Point point1, DlibDotNet.Point point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }
    }
}
