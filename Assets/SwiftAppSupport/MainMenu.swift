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
    var initialTab: String

    var body: some View {
        NavigationSplitView {
            List {
                NavigationLink("Overview", destination: Overview())
                NavigationLink("Add new entity", destination: NewPanel())
                NavigationLink("Settings", destination: Settings())
            }
            .navigationTitle("HomeassistantXR")
        } detail: {
            switch initialTab {
            case "Overview":
                Overview()
            case "NewPanel":
                NewPanel()
            case "Settings":
                Settings()
            default:
                Overview()
            }
        }
        .onChange(of: scenePhase) {
            if scenePhase == .background {
                CallCSharpCallback("windowClosed")
            }
        }
    }
}

#Preview(windowStyle: .automatic) {
    MainMenu(initialTab: "Overview")
}
