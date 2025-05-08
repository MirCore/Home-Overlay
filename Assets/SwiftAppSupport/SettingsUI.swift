import Foundation
import SwiftUI
#if canImport(UnityFramework)
import UnityFramework
#endif

#if canImport(PolySpatialRealityKit)
import PolySpatialRealityKit
#endif

struct Settings: View {
    @State var url: String = "http://homeassistant.local/"
    @State var port: String = "8123"
    @State var token: String = ""
    @ObservedObject var statusModel = ConnectionStatusModel.shared

    var body: some View {
        VStack(alignment: .leading){
            Text("Connection Settings")
                .font(.title3)
            
            Grid(alignment: .leading, horizontalSpacing: 20, verticalSpacing: 16) {
                GridRow {
                    Text("URL")
                    TextField(text: $url, prompt: Text("http://homeassistant.local/")) {
                        Text("Home Assistant URL")
                    }
                    .textContentType(.URL)
                    .keyboardType(.URL)
                    .disableAutocorrection(true)
                    .textFieldStyle(RoundedBorderTextFieldStyle())
                }
                GridRow {
                    Text("Port")
                    TextField(text: $port, prompt: Text("8123")) {
                        Text("Home Assistant Port")
                    }
                    .keyboardType(.numberPad)
                    .textInputAutocapitalization(.never)
                    .disableAutocorrection(true)
                    .textFieldStyle(RoundedBorderTextFieldStyle())
                    .onChange(of: port) { oldValue, newValue in
                        // Ensure only numeric input
                        port = newValue.filter { $0.isNumber }
                    }
                }
                GridRow {
                    Text("Token")
                    SecureField(text: $token, prompt: Text("Token")) {
                        Text("User Token")
                    }
                    .textInputAutocapitalization(.never)
                    .disableAutocorrection(true)
                    .textFieldStyle(RoundedBorderTextFieldStyle())
                }
                HStack {
                    Button("Test Connection") {
                        if url.isEmpty || port.isEmpty || token.isEmpty {
                            ConnectionStatusModel.shared.setMessage(status: 0, text: "All fields must be filled out.")
                        } else {
                            statusModel.message = nil
                            ConnectionStatusModel.shared.setMessage(status: -1, text: "Awaiting response...")
                            CallCSharpCallback("testConnection", url, port, token)
                        }
                    }
                    
                    if let message = statusModel.message {
                        let foregroundColor = (message.status == 200 || message.status == 201) ? Color.green : (message.status < 0 ? Color.primary : Color.red)
                        Text(message.text)
                            .font(.callout)
                            .foregroundColor(foregroundColor)
                            .padding(/*@START_MENU_TOKEN@*/.horizontal/*@END_MENU_TOKEN@*/)
                    }
                }
                
                Button("Save") {
                    CallCSharpCallback("saveConnection", url, port, token)
                }
            }
            .padding([.bottom, .trailing], 26)
            .navigationBarTitle("Settings")
            .onAppear {
                CallCSharpCallback("getConnectionValues")
                url = GetHassUrl().isEmpty ? url : GetHassUrl()
                port = GetHassPort().isEmpty ? port : GetHassPort()
                token = GetHassToken().isEmpty ? token : GetHassToken()
            }
            
            
            Text("Demo mode")
                .font(.title3)
            Text("This will create panels with demo values. These panels do not support any real functionality.")
                .font(.footnote)
                .foregroundColor(.gray)
            Button("Create demo panels") {
                CallCSharpCallback("createDemoPanels")
            }
            Spacer()
        }
        .padding([.leading, .bottom, .trailing], 26.0)
    }
}


#Preview(windowStyle: .automatic) {
    MainMenu(initialTab: "Settings")
}
