using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace emguTest1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var faceCascade = new CascadeClassifier(@"C:\Users\Yousef.J\source\repos\EmguCameraTest3Eye\EmguCameraTest3Eye\xmlsss\haarcascade_frontalface_default.xml");
            var eyeCascade = new CascadeClassifier(@"C:\Users\Yousef.J\source\repos\EmguCameraTest3Eye\EmguCameraTest3Eye\xmlsss\haarcascade_eye_tree_eyeglasses.xml");
            var vc = new VideoCapture(0, Emgu.CV.VideoCapture.API.DShow);

            Mat frame = new Mat();
            Mat frameGrey = new Mat();

            int blinkCount = 0;
            bool eyesPreviouslyDetected = true;
            bool faceDetected = false;

            const double minFacePercentage = 0.22; // Minimum percentage of frame size that the face should occupy

            // Define the cropping parameters
            const double cropFactor = 0.8; // 80% of the original size
            int cropWidth, cropHeight, offsetX, offsetY;
            Rectangle cropRect;

            while (vc.IsOpened)
            {
                vc.Read(frame);

                // Calculate the cropping dimensions based on the cropFactor
                cropWidth = (int)(frame.Width * cropFactor);
                cropHeight = (int)(frame.Height * cropFactor);
                offsetX = (frame.Width - cropWidth) / 2;
                offsetY = (frame.Height - cropHeight) / 2;

                // Define the cropping rectangle
                cropRect = new Rectangle(offsetX, offsetY, cropWidth, cropHeight);

                // Crop the frame
                Mat croppedFrame = new Mat(frame, cropRect);

                // Convert the cropped frame to grayscale
                CvInvoke.CvtColor(croppedFrame, frameGrey, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                CvInvoke.EqualizeHist(frameGrey, frameGrey);

                var faces = faceCascade.DetectMultiScale(frameGrey, 1.1, 4);

                faceDetected = faces.Length > 0;
                bool eyesDetected = false;

                if (faceDetected)
                {
                    foreach (var face in faces)
                    {
                        var faceROI = new Mat(frameGrey, face);
                        var eyes = eyeCascade.DetectMultiScale(faceROI, 1.1, 10, new Size(10, 10));

                        if (eyes.Length >= 2)
                        {
                            eyesDetected = true;

                            // Draw a rectangle around the face
                            CvInvoke.Rectangle(croppedFrame, face, new MCvScalar(0, 255, 0), 2);

                            // Draw circles for the eyes
                            foreach (var eye in eyes)
                            {
                                var eyeCenter = new Point(face.X + eye.X + eye.Width / 2, face.Y + eye.Y + eye.Height / 2);
                                int radius = (int)Math.Round((eye.Width + eye.Height) * 0.30);
                                CvInvoke.Circle(croppedFrame, eyeCenter, radius, new MCvScalar(255, 0, 0), 2);
                            }
                        }
                    }

                    // Blink detection logic
                    if (eyesPreviouslyDetected && !eyesDetected)
                    {
                        blinkCount++;
                    }

                    eyesPreviouslyDetected = eyesDetected;
                }
                else
                {
                    eyesPreviouslyDetected = true;
                    blinkCount = 0;
                }

                // Check if the detected face is large enough
                bool faceTooSmall = false;
                if (faceDetected)
                {
                    foreach (var face in faces)
                    {
                        double faceArea = face.Width * face.Height;
                        double frameArea = cropWidth * cropHeight; // Use cropped frame area
                        double facePercentage = faceArea / frameArea;

                        if (facePercentage < minFacePercentage)
                        {
                            faceTooSmall = true;
                            break;
                        }
                    }
                }

                // Display message if the face is too small
                if (faceTooSmall)
                {
                    string message = "Please come closer to the camera!";
                    CvInvoke.PutText(croppedFrame, message, new Point(croppedFrame.Width / 2 - 220, croppedFrame.Height / 2 + 100),
                                     Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.7,
                                     new MCvScalar(255, 255, 255), 2);
                }

                // Display the blink count on the screen
                CvInvoke.PutText(croppedFrame, $"Blinks: {blinkCount}", new Point(10, 60),
                                 Emgu.CV.CvEnum.FontFace.HersheySimplex, 1.0,
                                 new MCvScalar(255, 0, 0), 2);

                // Display the cropped frame
                CvInvoke.Imshow("Blink Detection", croppedFrame);

                // Exit if 'Esc' is pressed
                if (CvInvoke.WaitKey(1) == 27)
                {
                    break;
                }
            }
        }
    }
}
