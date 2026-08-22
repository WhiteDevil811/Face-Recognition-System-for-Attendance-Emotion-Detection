# 👁️ Face Recognition System for Attendance & Emotion Detection

An AI-powered, contactless attendance system for UTeM staff and students that combines **facial recognition** for authentication with **emotion detection** for wellbeing insight — removing the university's dependency on cards or mobile apps for clocking in.

---

## 📖 Overview

UTeM's attendance system currently relies on staff cards or a mobile app — but the card system is being discontinued, leaving mobile phones as the sole clock-in method. This introduces real friction: forgotten devices, low battery, weak connectivity, and app downtime all disrupt attendance recording and push staff back toward slow, error-prone manual logging.

This project replaces that dependency with a **device-independent, camera-based system**: a dedicated camera authenticates users by face and simultaneously reads their facial expression, so attendance recording doubles as a lightweight emotional wellbeing check-in — complete with a motivational response and a real-time admin dashboard.

---

## 🎯 Objectives

1. Develop a secure and efficient face recognition system for attendance recording
2. Build a facial emotion detection algorithm to analyze users' emotional state
3. Establish a motivation module that responds to detected emotions for user engagement
4. Design an admin dashboard for real-time monitoring and effective data management

---

## 🧩 System Modules

| Module | Function |
|---|---|
| **Staff/Student Database** | Stores personal details and reference facial images |
| **Face Recognition** | Authenticates identity for attendance clock-in |
| **Emotion Detection** | Analyzes facial expression during clock-in |
| **Motivation Module** | Returns a personalized message based on detected emotion |
| **Admin Dashboard** | Real-time monitoring, attendance log export, data management |

**Target users:** UTeM staff, UTeM students, UTeM administrators

---

## 🧠 Technical Approach

### Face Recognition — HOG + ResNet

1. **Detection:** Histogram of Oriented Gradients (HOG) locates the face in the camera frame
2. **Embedding:** A ResNet (Deep Residual Network) converts detected facial features into a 128-dimensional embedding vector
3. **Verification:** The embedding is compared against the staff/student database using **Euclidean distance** to confirm identity

### Emotion Detection — CNN

1. The face's Region of Interest (ROI) is extracted and converted to grayscale
2. A **Convolutional Neural Network (CNN)** classifies the expression into categories such as *Happy, Sad, Neutral, Stressed*

### Motivation Logic — Rule-Based System

- Detected emotion maps to a predefined set of motivational quotes/greetings
- Example: a detected **Sad** or **Stressed** state triggers a supportive message shown to the user

---

## 🏗️ System Architecture

The system follows a **modular architecture** — each module (face recognition, emotion detection, motivation, admin dashboard) handles one responsibility and operates semi-independently, communicating through a centralized database. This keeps the system flexible, easier to maintain, and lets individual components be upgraded without breaking the rest.

```
        ┌─────────────────────┐
        │   Camera Input       │
        │ (real-time video)    │
        └──────────┬───────────┘
                   ↓
        ┌─────────────────────┐
        │  Face Recognition    │──→ Staff/Student Database
        │   (HOG + ResNet)      │      (match by embedding)
        └──────────┬───────────┘
                   ↓ (identity confirmed)
        ┌─────────────────────┐
        │  Emotion Detection    │
        │       (CNN)           │
        └──────────┬───────────┘
                   ↓
        ┌─────────────────────┐
        │  Motivation Module     │──→ Personalized response
        │  (rule-based mapping)  │      shown to user
        └──────────┬───────────┘
                   ↓
        ┌─────────────────────┐
        │   Admin Dashboard      │
        │ (logs, monitoring,     │
        │  data management)      │
        └─────────────────────┘
```

---

## ⚙️ Functional Requirements

1. Capture real-time video via an external camera connected over wired cable
2. Identify the user by matching their face against stored database images
3. Detect the user's current emotion during clock-in
4. Display a personalized motivational message based on detected emotion
5. Allow admins to export attendance logs and manage student/staff data via the dashboard

---

## 🛠️ Development Methodology

Built using **Iterative Prototyping**, chosen for its fit with integrating multiple complex AI components incrementally:

1. **Planning & Requirements** — hardware constraints (camera, wire cable, switch) and software requirements (Python, OpenCV) analysis
2. **Prototype Design** — initial admin dashboard interface and database structure
3. **Iterative Implementation**
   - *Iteration 1:* Face recognition (replacing card/mobile clock-in)
   - *Iteration 2:* Emotion detection integration
   - *Iteration 3:* Motivation module triggered by detected emotion
4. **Testing & Refinement** — lab-environment testing with the specified laptop/camera setup

---

## 🖥️ Requirements

| Category | Requirements |
|---|---|
| **Software** | Python, C++, OpenCV |
| **Hardware** | Camera, wire cable, switch, monitor, laptop, internet access |

---

## 🚀 Getting Started

> Setup instructions will depend on the final implementation structure of this repo. As a starting point:

```bash
# Clone the repository
git clone <this-repo-url>
cd face-recognition-attendance-emotion

# Install dependencies
pip install opencv-python face_recognition dlib tensorflow numpy

# Run the system
python main.py
```

*(Update dependencies and entry point to match the actual implementation files in this repo.)*

---

## 💡 Project Significance

- **Hands-free, device-independent** attendance — no reliance on cards or personal phones
- **Reduced technical friction** from forgotten/malfunctioning/low-battery devices
- **Wellbeing insight** — emotion detection surfaces signals that a purely transactional system would miss, enabling timely support
- **Centralized, real-time management** for administrators via the dashboard

---

## 👥 Team

| Name | Matric No. |
|---|---|
| Dee Ying A/P Kok Hoe |
| Thenaa A/L Hari Kumar  |
| Quah Yi Sheng  |
| Muhammad Hafizuddin Bin Kamarul Hatta  |

---
