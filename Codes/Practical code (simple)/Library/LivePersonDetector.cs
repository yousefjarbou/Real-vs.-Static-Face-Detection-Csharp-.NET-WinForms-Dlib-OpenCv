using Point = System.Drawing.Point;
using DlibDotNet;
using DlibDotNet.Extensions;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace LivePersonDetectionLibrary
{
    public class LivePersonDetector
    {
        private ShapePredictor _shapePredictor;
        private FrontalFaceDetector _faceDetector;
        private VideoCapture _videoCapture;
        private int _blinkCount;
        private double _difference;
        private Stopwatch _stopwatch;
        private List<double> _earReadings;
        private bool _detectionComplete;

        // Constructor
        public LivePersonDetector()
        {
            //shape_predictor_68_face_landmarks.dat file that is the trained model that is used in DLIB
            //https://github.com/italojs/facial-landmarks-recognition/blob/master/shape_predictor_68_face_landmarks.dat
            _shapePredictor = ShapePredictor.Deserialize(@"shape_predictor_68_face_landmarks.dat");

            _faceDetector = Dlib.GetFrontalFaceDetector();
            _videoCapture = new VideoCapture(0, VideoCapture.API.DShow);
            _earReadings = new List<double>();
        }

        // Method to start detection
        public bool StartDetection(int detectionDurationInSeconds = 15, int blinkThreshold = 5, double differenceThreshold = 0.5)
        {
            _blinkCount = 0;
            _difference = 0;
            _detectionComplete = false;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            while (_stopwatch.Elapsed.TotalSeconds < detectionDurationInSeconds && !_detectionComplete)
            {
                using (var frame = new Mat())
                {
                    _videoCapture.Read(frame);

                    Bitmap bitmap = MatToBitmap(frame);
                    var dlibImage = BitmapExtensions.ToArray2D<RgbPixel>(bitmap);
                    var faces = _faceDetector.Operator(dlibImage);

                    if (faces.Length == 0)
                    {
                        _blinkCount = 0;
                        _earReadings.Clear();
                    }

                    foreach (var face in faces)
                    {
                        var shape = _shapePredictor.Detect(dlibImage, face);
                        var leftEAR = CalculateEAR(shape, 36, 37, 38, 41, 40, 39);
                        var rightEAR = CalculateEAR(shape, 42, 43, 44, 47, 46, 45);

                        double ear = (leftEAR + rightEAR) / 2.0;
                        _earReadings.Add(ear);

                        if (_earReadings.Count == 20)
                        {
                            _earReadings.Sort();
                            double min1 = _earReadings[0];
                            double min2 = _earReadings[1];
                            double max1 = _earReadings[19];
                            double max2 = _earReadings[18];

                            _difference = (max1 + max2) - (min1 + min2);
                            if (_difference > differenceThreshold)
                            {
                                _blinkCount++;
                            }
                            _earReadings.Clear();
                        }

                        if (_blinkCount >= blinkThreshold)
                        {
                            _detectionComplete = true;
                            _videoCapture.Dispose();
                            CvInvoke.DestroyAllWindows(); // Close the video window when done
                            break;
                        }
                        // Draw landmarks and face bounding box
                        for (uint i = 0; i < shape.Parts; i++) // Use 'uint' for indexing
                        {
                            var point = new Point(shape.GetPart(i).X, shape.GetPart(i).Y);
                            CvInvoke.Circle(frame, point, 2, new MCvScalar(0, 255, 0), -1);
                        }

                        CvInvoke.Rectangle(frame, new System.Drawing.Rectangle(face.Left, face.Top, (int)face.Width, (int)face.Height), new MCvScalar(0, 255, 0), 2);

                        // Display blink count
                        CvInvoke.PutText(frame, $"Blinks: {_blinkCount}", new Point(10, 60), FontFace.HersheySimplex, 1.0, new MCvScalar(255, 0, 0), 2);
                    }
                    CvInvoke.Imshow("Blink Detection with Info Row", frame);
                    CvInvoke.WaitKey(1);
                }
            }

            _videoCapture.Dispose();
            CvInvoke.DestroyAllWindows(); // Close the video window when done
            return _blinkCount >= blinkThreshold;
        }

        private Bitmap MatToBitmap(Mat mat)
        {
            Bitmap bitmap = new Bitmap(mat.Width, mat.Height, PixelFormat.Format24bppRgb);
            BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

            int byteCount = mat.Width * mat.Height * mat.ElementSize;
            byte[] data = new byte[byteCount];
            Marshal.Copy(mat.DataPointer, data, 0, byteCount);
            Marshal.Copy(data, 0, bitmapData.Scan0, byteCount);

            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        private double CalculateEAR(FullObjectDetection shape, int p1, int p2, int p3, int p4, int p5, int p6)
        {
            double a = Distance(shape.GetPart((uint)p2), shape.GetPart((uint)p6));
            double b = Distance(shape.GetPart((uint)p3), shape.GetPart((uint)p5));
            double c = Distance(shape.GetPart((uint)p1), shape.GetPart((uint)p4));
            return (a + b) / (2.0 * c);
        }

        private double Distance(DlibDotNet.Point point1, DlibDotNet.Point point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }
    }
}
