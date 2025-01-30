# Real vs. Static Face Detection C#-.NET-WinForms-Dlib-OpenCv

This project was developed as part of my internship at **SEDCO** during the summer of 2024. It is a **C# .NET Windows application** designed to differentiate between a **real person** and a **static image** using webcam-based face detection. The system enhances security for facial recognition applications by verifying user **liveness** through **eye-blink detection**.

To make this feature reusable, I converted the application into a **DLL**, allowing easy integration into other projects. The function, when called, opens a camera window for a short time, processes the live feed, and then **returns a boolean value** (`true` for a real person, `false` for a static image). This modular design enables seamless incorporation into various security and authentication systems.

This DLL was successfully **integrated into my main project** during the internship—**[Zain Service App C#-.NET-WinForms-Dlib-OpenCv-XML-SQL SEDCO](https://github.com/yousefjarbou/Zain-Service-App-Csharp-.NET-WinForms-Dlib-OpenCv-XML-SQL-SEDCO)**.

---

## 🔹 Features

- **Real vs. Static Face Detection**
  - Detects if a face is **real** or a **photo** using **blink detection**.
  - Uses **Eye Aspect Ratio (EAR)** for accurate liveness detection.
  - Works in **real-time** with a built-in **time-out mechanism**.

- **DLL for Easy Integration**
  - The detection module is implemented as a **standalone DLL**.
  - When the function is called, it opens the webcam and processes the face in real-time.
  - After the time-out, it **returns `true` or `false`**, making it adaptable to various applications.

- **Advanced Computer Vision Techniques**
  - **DlibDotNet**: Landmark detection for eye tracking.
  - **OpenCvSharp**: Image processing and real-time frame analysis.
  - **Blink Counting Algorithm**: Differentiates real users from photos or deepfakes.

- **Scalability & Code Flexibility**
  - The project follows **good OOP principles** such as:
    - **Single Responsibility Principle**: Each module handles one specific task.
    - **Open-Closed Principle**: The system is **extensible** without modifying existing code.
  - This ensures **easy maintainability** and future upgrades.

---

## 🛠 Technology Stack

- **Programming Language:** C#
- **Framework:** .NET Windows Forms
- **Libraries Used:**
  - **OpenCvSharp** → For image processing.
  - **DlibDotNet** → For facial landmark detection.
  - **Emgu.CV** → Alternative face detection library.

---

## 🎥 Videos

- [**Demo Video of Face Detection in Action**](https://youtu.be/QTPNf8ESYG4?si=nytJMi6DZ_-hayzH)  
  *(A demonstration of the DLL detecting a real person vs. a static image.)*

- [**Integration in Zain Service App**](https://youtu.be/5NQG6BgDbJw?si=f0_mb6p5BV2qYfEB)  
  *(Shows how the DLL was integrated into the Zain Service App for real-time authentication.)*

---

## 📝 How It Works

1. **Face Detection & Eye Tracking**
   - The webcam captures frames, and **Dlib** detects the facial landmarks.
   - It **calculates the Eye Aspect Ratio (EAR)** to monitor blinking.

2. **Blink-Based Liveness Verification**
   - If blinking is detected within the **time-out window**, the system confirms a **real person**.
   - If no blinks are detected, the system **flags the face as static**.

3. **DLL Functionality**
   - Calling the DLL **opens a camera window** and starts the detection.
   - After the time-out, it **returns a boolean**:
     - ✅ **True** → Real person detected.
     - ❌ **False** → Static image detected.

---

## 🔗 Related Project

🔹 This face detection module was successfully integrated into my **Customer Service Application**:  
👉 **[Zain Service App C#-.NET-WinForms-Dlib-OpenCv-XML-SQL SEDCO](https://github.com/your-zain-app-repo)**

---

## 🎨 UI & Design

- Built using **Windows Forms**, showcasing **dynamic UI design**.
- Replicated **Zain’s branding**:
  - Extracted **colors, logos, and patterns** directly from the **Zain website**.
  - Ensured **clean, professional UI consistency**.

---

## 🚀 Future Enhancements

- **Adaptive EAR Thresholding** → Adjust based on lighting conditions.
- **Multi-Factor Liveness Checks** → Add head movement challenges for extra security.
- **Hardware Integration** → Support for **depth sensors** to improve spoof detection.

---

This project demonstrates **advanced facial recognition techniques** and **modular software design**, making it adaptable for **authentication, fraud prevention, and secure access control**. 🚀
