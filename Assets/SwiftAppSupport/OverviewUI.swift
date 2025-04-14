import Foundation
import SwiftUI
#if canImport(UnityFramework)
import UnityFramework
#endif

#if canImport(PolySpatialRealityKit)
import PolySpatialRealityKit
#endif

struct Overview: View {
    @State private var panels: [Item] = []
    @ObservedObject var statusModel = ConnectionStatusModel.shared
    
    var connectionStatus: String {
        if let message = statusModel.message {
            if (message.uri == "") {
                return "No connection settings found. Please set them in the settings menu."
            } else if (message.status == 200 || message.status == 201) {
                return "Successfully connected to: \(message.uri)"
            } else {
                return "Failed to connect to: \(message.uri)"
            }
        } else {
            return "Waiting for connection status..."
        }
    }
    
    func reloadPanels() {
        CallCSharpCallback("getPanels")
        panels = panelsData
    }
    
    var body: some View {
        VStack(alignment: .leading, spacing: 20) {
            
            Text(connectionStatus)
                .padding(/*@START_MENU_TOKEN@*/.bottom/*@END_MENU_TOKEN@*/)
            
            List {
                Section(header: Text("Active Panels")) {
                    ForEach(panels.filter { $0.active }.sorted(by: { $0.entityId < $1.entityId }), id: \.panelId) { item in
                        OverviewListRow(item: item, refreshPanels: reloadPanels)
                    }
                }

                Section(header: Text("Inactive Panels")) {
                    ForEach(panels.filter { !$0.active }.sorted(by: { $0.entityId < $1.entityId }), id: \.panelId) { item in
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
            CallCSharpCallback("getConnectionStatus")
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
