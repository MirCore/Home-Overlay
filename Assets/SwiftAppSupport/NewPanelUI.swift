import Foundation
import SwiftUI
#if canImport(UnityFramework)
import UnityFramework
#endif

#if canImport(PolySpatialRealityKit)
import PolySpatialRealityKit
#endif

struct NewPanel: View {
    @State private var searchText: String = ""
    @State private var selectedEntityId: String? = nil
    @State private var filteredEntities: [Item] = []

    @State var entityTypes = ["All types", "Light", "Sensor", "Switch", "Camera"]
    @State private var selectedDeviceType = "All types"
    
    var body: some View {
        Grid(alignment: .leading, horizontalSpacing: 20, verticalSpacing: 16) {
            
            // Dropdown for Device Type
            GridRow {
                Text("Filter device types")
                Picker("Device Type", selection: $selectedDeviceType) {
                    ForEach(entityTypes, id: \.self) {
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
                    createEntity(selectedEntityId ?? "")
                }
                .disabled(selectedEntityId == nil)
            }
            Spacer()
        }
        .padding([.leading, .bottom, .trailing], 26)
        .navigationTitle("Add new entity")
        .onAppear {
            CallCSharpCallback("getEntities")
            entityTypes = entityTypesData
            filterEntities(searchText: "")
        }
    }
    
    private func filterEntities(searchText: String) {
        filteredEntities = entitiesData.filter { item in
            let matchesDeviceType = selectedDeviceType == entityTypes[0] || item.entityId.hasPrefix(selectedDeviceType.lowercased().replacingOccurrences(of: " ", with: "_"))
            let matchesSearchText = searchText.isEmpty || item.entityId.localizedCaseInsensitiveContains(searchText) || item.name.localizedCaseInsensitiveContains(searchText)
            return matchesDeviceType && matchesSearchText
        }
    }
    
    private func createEntity(_ entityId: String) {
        CallCSharpCallback("createEntity", entityId)
    }
}

#Preview(windowStyle: .automatic) {
    MainMenu(initialTab: "NewPanel")
}
