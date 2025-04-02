import Foundation
import SwiftUI
#if canImport(UnityFramework)
import UnityFramework
#endif

#if canImport(PolySpatialRealityKit)
import PolySpatialRealityKit
#endif


struct MainMenu: View {
    @Environment(\.scenePhase) private var scenePhase

    var body: some View {
        NavigationSplitView {
            List {
                NavigationLink("Overview", destination: Overview())
                NavigationLink("Add new entity", destination: NewEntity())
                NavigationLink("Settings", destination: Settings())
            }.navigationTitle("HomeassistantXR")
        } detail: {Overview()}
        .onDisappear { CallCSharpCallback("windowClosed") }
        .onChange(of: scenePhase) {
            if scenePhase == .background {
                print("Swift window closed")
                CallCSharpCallback("windowClosed")
            }
        }
    }
}

struct Overview: View {
    @State private var panels: [Item] = []
    @ObservedObject var statusModel = ConnectionStatusModel.shared
    
    var connectionStatus: String {
        if let message = statusModel.message {
            return (message.status == 200 || message.status == 201) ? "Successfully connected to: \(message.savedUri)" : "Failed to connect to: \(message.savedUri)"
        }
        return "Waiting for connection status..."
    }
    
    func reloadPanels() {
        CallCSharpCallback("getPanels")
        panels = panelsData
    }
    
    var body: some View {
        VStack(alignment: .leading, spacing: 20) {
            
            Text(connectionStatus)
                .padding(/*@START_MENU_TOKEN@*/.bottom/*@END_MENU_TOKEN@*/)
            /*Text("\(panels.count) window\(panels.count != 1 ? "s" : "") created")*/
            
            List {
                Section(header: Text("Active Panels")) {
                    ForEach(panels.filter { $0.active }.sorted(by: { $0.entityId < $1.entityId }), id: \.entityId) { item in
                        OverviewListRow(item: item, refreshPanels: reloadPanels)
                    }
                }

                Section(header: Text("Inactive Panels")) {
                    ForEach(panels.filter { !$0.active }.sorted(by: { $0.entityId < $1.entityId }), id: \.entityId) { item in
                        OverviewListRow(item: item, refreshPanels: reloadPanels)
                    }
                }
            }
            .padding(.horizontal, -26)
            
            Text("Panels are created only when their position is found.")
                .font(.footnote)
                .foregroundColor(Color.gray)
        }
        .padding([.leading, .bottom, .trailing], 26)
        .navigationTitle("Overview")
        .onAppear {
            CallCSharpCallback("GetConnectionStatus")
            CallCSharpCallback("getPanels")
            panels = panelsData
        }
    }
    
    struct OverviewListRow: View {
        let item: Item
        let refreshPanels: () -> Void

        var body: some View {
            Grid(alignment: .leading) {
                GridRow {
                    VStack(alignment: .leading) {
                        Text(item.name)
                        Text(item.entityId)
                            .font(.caption)
                            .foregroundColor(.gray)
                    }

                    Spacer()

                    if item.active {
                        Button(action: {
                            CallCSharpCallback("highlightPanel", item.panelId)
                        }) {
                            Label("Highlight Panel", systemImage: "magnifyingglass")
                                .padding()
                        }
                        .buttonStyle(.plain)
                    }

                    Button(action: {
                        CallCSharpCallback("deletePanel", item.panelId)
                        refreshPanels()
                    }) {
                        Label("Delete Panel", systemImage: "trash")
                            .padding()
                    }
                    .buttonStyle(.plain)
                    .tint(.red)
                }
            }
        }
    }
}




struct NewEntity: View {
    @State private var searchText: String = ""
    @State private var selectedEntityId: String = ""
    @State private var filteredEntities: [Item] = []

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
            List(filteredEntities.sorted(by: { $0.name < $1.name }), id: \.entityId) { item in
                 Button(action: {
                     selectedEntityId = item.entityId
                 }) {
                     VStack(alignment: .leading){
                         Text(item.name)
                         Text(item.entityId)
                             .font(.caption)
                             .foregroundColor(Color.gray)
                     }
                     .padding(.vertical, 2)
                 }.listRowBackground(selectedEntityId == item.entityId ? Color.white.opacity(0.1) : .none)
             }.padding(.horizontal, -26)

            HStack{
                Button("Create selected entity"){
                    createEntity(selectedEntityId)
                }
            }
            Spacer()
        }
        .padding([.leading, .bottom, .trailing], 26)
        .navigationTitle("Add new entity")
        .onAppear {
            CallCSharpCallback("getEntities")
            filterEntities(searchText: "")
        }
    }
    
    private func filterEntities(searchText: String) {
        filteredEntities = entitiesData.filter { item in
            let matchesDeviceType = selectedDeviceType == "All types" || item.entityId.hasPrefix(selectedDeviceType.lowercased())
            let matchesSearchText = searchText.isEmpty || item.entityId.localizedCaseInsensitiveContains(searchText) || item.name.localizedCaseInsensitiveContains(searchText)
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
    @ObservedObject var statusModel = ConnectionStatusModel.shared

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
                        Task { @MainActor in
                            ConnectionStatusModel.shared.setMessage(status: 0, text: "All fields must be filled out.")
                        }
                    } else {
                        statusModel.message = nil
                        print("Testing connection with URL: \(url), Port: \(port), Token: \(token)")
                        
                        Task { @MainActor in
                            ConnectionStatusModel.shared.setMessage(status: -1, text: "Awaiting response...")
                        }
                        CallCSharpCallback("testConnection", url, port, token)
                    }
                }
            
                if let message = statusModel.message {
                    let foregroundColor = (message.status == 200 || message.status == 201) ? Color.green : (message.status < 0 ? Color.primary : Color.red)
                    Text(message.text)
                        .font(.callout)
                        .foregroundColor(foregroundColor)
                        .padding(/*@START_MENU_TOKEN@*/.horizontal/*@END_MENU_TOKEN@*/)
                        .onAppear {
                            // Schedule the message to disappear after 10 seconds
                            //DispatchQueue.main.asyncAfter(deadline: .now() + 10) {
                            //    self.statusModel.message = nil
                            //}
                        }
                }
            }
            
            Button("Save") {
                print("Saving connection with URL: \(url), Port: \(port), Token: \(token)")
                CallCSharpCallback("saveConnection", url, port, token)
            }
            Spacer()
                
        }
        .padding([.leading, .bottom, .trailing], 26)
        .navigationBarTitle("Settings")
        .onAppear {
            // Call the public function that was defined in SwiftUIPlugin inside UnityFramework
            CallCSharpCallback("getConnectionValues")
            url = GetHassUrl().isEmpty ? url : GetHassUrl()
            port = GetHassPort().isEmpty ? port : GetHassPort()
            token = GetHassToken().isEmpty ? token : GetHassToken()
        }
        .onDisappear {
            //CallCSharpCallback("closed")
        }
    }
}

#Preview(windowStyle: .automatic) {
    MainMenu()
}

