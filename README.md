# Home Overlay for Apple Vision Pro
**Spatial smart home control in mixed reality**

![Banner](https://github.com/user-attachments/assets/e00a8946-cd3f-42cb-b72f-462d73caf8aa)

Home Overlay transforms your living space into an interactive smart home control center through the power of Apple Vision Pro's mixed reality capabilities. Control your lights, check the weather, and view your calendar - all seamlessly integrated into your physical environment.

## 🌟 Features

### 🎯 Spatial Smart Home Controls
- Intuitive gesture-based control of smart home devices
- Panels placed in real-world space
- Panels remember their position — they return to the correct spot when the app is restarted
- Mixed reality UI that feels natural and ambient

### 💡 Light & Ambience
- Real-time brightness an color adjustment
- Instant feedback for light changes
- Works with all Home Assistant light entities

### 🏠 Entities Overview
- View all connected lights, sensors and other entities
- Add panels for any Home Assistant entity

### 📅 Calendar Integration
- See upcoming events from your synced calendar

### ⛅ Weather Information
- Current conditions and weather forecasts

### 🔒 Local & Private
- Connects directly to your Home Assistant instance
- No cloud, no tracking, no data leaves your home

## 🔧 Requirements

- Apple Vision Pro
- Home Assistant instance
- Network connectivity between Vision Pro and Home Assistant
  
---

## 🚀 Getting Started

### Demo Mode
Want to try the app without a Home Assistant setup? No problem!
1. Install Home Overlay from the Apple Vision Pro App Store
2. Launch the app and open the menu by clicking the floating icon
3. Open the **Settings** menu
5. Use the **Create demo panels** option to create sample panels
6. Experiment with all features using simulated data

### Home Assistant Connection
Ready to connect to your smart home?
1. Open the **Settings** menu
2. Connect to your Home Assistant instance:
   - Enter your Home Assistant URL and Port
   - Provide your Long-Lived Access Token (can be created in your Home Assistant profile)
   - The token can be copy-pasted by double-tapping the text field
3. After successful connection, go to **Add new entity** to start placing panels for your devices

> Note: To create a Long-Lived Access Token in Home Assistant:
> 1. Go to your Home Assistant Profile
> 2. Open the Security tab and scroll down to "Long-lived access tokens"
> 3. Click "Create Token"

## 📋 Panel Management

- Create panels for different devices and information types
- Each panel can be freely positioned in your space
- Available panel types:
  - 💡 Light and button controls (interactive)
  - ⛅ Weather (read-only)
  - 📅 Calendar (read-only)
  - 🔍 Sensor data (read-only)
- Panels behaviour can be edited through the panel settings
- Use pinch gestures to resize panels
- Use the alignment setting to align panels with nearby walls

## 🔐 Privacy

- 100% local communication with Home Assistant
- No third-party servers or analytics
- Your data stays in your home

## 🛠 Tech Stack

- Built with Unity
- Native SwiftUI integration
- ARKit for spatial understanding and persistent real-world anchors
- Home Assistant REST API integration

## 📱 Support

For support, feature requests, or bug reports, please:
- [Open an issue](https://github.com/mircore/home-overlay/issues)
- [Join the discussion](https://github.com/mircore/home-overlay/discussions)
