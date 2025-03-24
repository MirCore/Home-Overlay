//
// This custom View is referenced by SwiftUISampleInjectedScene
// to provide the body of a WindowGroup. It's part of the Unity-VisionOS
// target because it lives inside a "SwiftAppSupport" directory (and Unity
// will move it to that target).
//

import Foundation
import SwiftUI
import UnityFramework
import PolySpatialRealityKit

struct SettingsWindow: View {
    @State var url: String = "http://homeassistant.local/"
    @State var port: String = "8123"
    @State var token: String = ""

    var body: some View {
        VStack {
            Text("Settings")
                .font(.headline)
                .padding(.bottom)
            Divider()
                .padding(10)
            Form{
                TextField(text: $url, prompt: Text("http://homeassistant.local/")) {
                    Text("Home Assistant URL")
                }
                    .keyboardType(.URL)
                    .textInputAutocapitalization(.never)
                    .disableAutocorrection(true)
                TextField(text: $port, prompt: Text("8123")) {
                    Text("Home Assistant Port")
                }
                    .keyboardType(.numberPad)
                    .textInputAutocapitalization(.never)
                    .disableAutocorrection(true)
                    .onChange(of: port) { oldValue, newValue in
                    // Ensure only numeric input
                    port = newValue.filter { $0.isNumber }
                }
                SecureField(text: $token, prompt: Text("Token")) {
                    Text("User Token")
                }
                    .textInputAutocapitalization(.never)
                    .disableAutocorrection(true)
                
                Button("Test Connection") {
                    print("Testing connection with URL: \(url), Port: \(port), Token: \(token)")
                    CallCSharpCallback("test connection", url, port, token)
                }
                
                Button("Save") {
                    print("Saving connection with URL: \(url), Port: \(port), Token: \(token)")
                    CallCSharpCallback("save connection", url, port, token)
                }
            }
        }
        .onAppear {
            // Call the public function that was defined in SwiftUIPlugin
            // inside UnityFramework
            //CallCSharpCallback("appeared")
            url = GetHassUrl().isEmpty ? url : GetHassUrl()
            port = GetHassPort().isEmpty ? port : GetHassPort()
            token = GetHassToken().isEmpty ? token : GetHassToken()
        }
        .onDisappear {
            //CallCSharpCallback("closed")
        }
        .padding(10)
    }
}

#Preview(windowStyle: .automatic) {
    SettingsWindow()
}

