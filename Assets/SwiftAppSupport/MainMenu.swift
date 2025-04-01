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

struct MainMenu: View {
    
    var body: some View {
        NavigationSplitView {
            List {
                NavigationLink("Overview", destination: Overview())
                NavigationLink("Add new entity", destination: NewEntity())
                NavigationLink("Settings", destination: Settings())
            }.navigationTitle("HomeassistantXR")
        } detail: {Settings()}
    }
}
struct Overview: View {
    var body: some View {
        VStack {
            
        }.navigationTitle("Overview")
    }
}

struct NewEntity: View {
    @State private var searchText: String = ""
    @State private var selectedEntityId: String = ""
    @State private var filteredEntities: [String: String] = [:]

    var deviceTypes = ["All types", "Light", "Sensor", "Switch", "Camera"]
    @State private var selectedDeviceType = "All types"
    
    var body: some View {
        Grid(alignment: .leading, horizontalSpacing: 20, verticalSpacing: 16) {
            
            // Dropdown for Device Type
            GridRow {
                Text("Filter device types")
                Picker("Device Type", selection: $selectedDeviceType) {
                    ForEach(deviceTypes, id: \.self) {
                        Text($0)
                    }
                }
                .onChange(of: selectedDeviceType) { oldValue, newValue in
                    filterEntities(searchText: searchText)
                }
            }
            
            // Search Field
            GridRow{
                Text("Text filter")
                TextField("Search entities...", text: $searchText)
                    .textFieldStyle(RoundedBorderTextFieldStyle())
                    .onChange(of: searchText) { oldValue, newValue in
                        filterEntities(searchText: newValue)
                    }
            }
            
            // VStack for Entities
             List(filteredEntities.sorted(by: { $0.value < $1.value }), id: \.key) { key, value in
                 Button(action: {
                     selectedEntityId = key
                 }) {
                     VStack(alignment: .leading){
                         Text(value)
                         Text(key)
                             .font(.caption)
                             .foregroundColor(Color.gray)
                     }
                     .padding(.vertical, 2)
                 }.listRowBackground(selectedEntityId == key ? Color.white.opacity(0.1) : .none)
             }.padding(.horizontal, -26)

            HStack{
                Button("Create selected entity"){
                    createEntity(selectedEntityId)
                }
            }
            Spacer()
        }
        .padding(26)
        .navigationTitle("Add new entity")
        .onAppear {
            CallCSharpCallback("getEntities")
            filterEntities(searchText: "")
        }
    }
    
    private func filterEntities(searchText: String) {
        filteredEntities = entitiesDict.filter { key, value in
            let matchesDeviceType = selectedDeviceType == "All types" || key.hasPrefix(selectedDeviceType.lowercased())
            let matchesSearchText = searchText.isEmpty || value.localizedCaseInsensitiveContains(searchText) || key.localizedCaseInsensitiveContains(searchText)
            return matchesDeviceType && matchesSearchText
        }
    }
    
    private func createEntity(_ entityId: String) {
        CallCSharpCallback("createEntity", entityId)
    }
}
    
struct Settings: View {
    @State var url: String = "http://homeassistant.local/"
    @State var port: String = "8123"
    @State var token: String = ""
    @State var message: Message?
    
    var body: some View {
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
                        message = Message (status: 0, text: "All fields must be filled out.")
                    } else {
                        message = nil
                        print("Testing connection with URL: \(url), Port: \(port), Token: \(token)")
                        CallCSharpCallback("test connection", url, port, token)
                    }
                }
            
                if let message = message {
                    var color = message.status == 200 || message.status == 201 ? Color.green : Color.red
                    var text = message.status > 201 ? "Connection failed: \(message.text) (\(message.status))" : message.text
                    Text(text)
                        .font(.callout)
                        .foregroundColor(color)
                        .padding(/*@START_MENU_TOKEN@*/.horizontal/*@END_MENU_TOKEN@*/)
                        .onAppear {
                            // Schedule the message to disappear after 10 seconds
                            DispatchQueue.main.asyncAfter(deadline: .now() + 10) {
                                self.message = nil
                            }
                        }
                }
            }
            
            Button("Save") {
                print("Saving connection with URL: \(url), Port: \(port), Token: \(token)")
                CallCSharpCallback("save connection", url, port, token)
            }
            Spacer()
                
        }
        .padding(26)
        .navigationBarTitle("Settings")
        .onAppear {
            // Call the public function that was defined in SwiftUIPlugin
            // inside UnityFramework
            CallCSharpCallback("getConnectionValues")
            url = GetHassUrl().isEmpty ? url : GetHassUrl()
            port = GetHassPort().isEmpty ? port : GetHassPort()
            token = GetHassToken().isEmpty ? token : GetHassToken()
        }
        .onDisappear {
            //CallCSharpCallback("closed")
        }
    }
    
    // Define a struct to hold the status and message
    struct Message {
        var status: Int
        var text: String
    }
}

#Preview(windowStyle: .automatic) {
    MainMenu()
}

