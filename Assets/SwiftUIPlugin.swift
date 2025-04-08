import Foundation
import SwiftUI

typealias CallbackDelegateType = @convention(c) (UnsafePointer<CChar>, UnsafePointer<CChar>, UnsafePointer<CChar>, UnsafePointer<CChar>) -> Void

var callbackDelegate: CallbackDelegateType? = nil
var hassUrl = "http://homeassistant.local/"
var hassPort = "8123"
var hassToken = ""
var connectionStatus = "Checking connection..."
public var entitiesData: [Item] = []
public var entityTypesData: [String] = []
public var panelsData: [Item] = []

@_cdecl("SetNativeCallback")
func setNativeCallback(_ delegate: CallbackDelegateType)
{
    print("############ SET NATIVE CALLBACK")
    callbackDelegate = delegate
}

public func CallCSharpCallback(_ str: String, _ arg0: String = "", _ arg1: String = "", _ arg2: String = "")
{
    guard let callbackDelegate = callbackDelegate else {
        print("ERROR: Callback delegate is nil")
        return
    }

    str.withCString { cString in
        arg0.withCString { cArg0 in
            arg1.withCString{ cArg1 in
                arg2.withCString { cArg2 in
                    callbackDelegate(cString, cArg0, cArg1, cArg2)
                }
            }
        }
    }
}

@_cdecl("OpenSwiftUIWindow")
func openSwiftUIWindow(_ cWindow: UnsafePointer<CChar>, _ cTab: UnsafePointer<CChar>)
{
    let openWindow = EnvironmentValues().openWindow

    let window = String(cString: cWindow)
    let tab = String(cString: cTab)
    print("############ OPEN WINDOW \(window), with tab \(tab)")
    openWindow(id: window, value: tab)
}

@_cdecl("SetSwiftUIConnectionValues")
func setSwiftUIConnectionValues(_ cUrl: UnsafePointer<CChar>, _ cPort: UnsafePointer<CChar>, _ cToken: UnsafePointer<CChar>)
{
    hassUrl = String(cString: cUrl);
    hassPort = String(cString: cPort);
    hassToken = String(cString: cToken);

    if (hassPort == "0")
    {
        hassPort = "8123";
    }
}


@_cdecl("SetSwiftUIConnectionStatus")
public func setSwiftUIConnectionStatus(_ status: Int32, _ cMessage: UnsafePointer<CChar>, _ cUri: UnsafePointer<CChar>) {
    let message = String(cString: cMessage)
    let uri = String(cString: cUri)
    print("Setting message from Unity: \(status) - \(message) - \(uri)")
    
    Task { @MainActor in
        ConnectionStatusModel.shared.setMessage(status: Int(status), text: message, uri: uri)
    }
}

@_cdecl("SetSwiftUIHassEntities")
func setSwiftUIHassEntities(_ cEntities: UnsafePointer<CChar>, _ cEntityTypes: UnsafePointer<CChar>) {
    let entitiesString = String(cString: cEntities)
    let entityTypesString = String(cString: cEntityTypes)

    // Attempt to convert the string to Data
    if let data = entitiesString.data(using: .utf8) {
        do {
            // Decode the JSON data into the ItemsWrapper structure
            let decoded = try JSONDecoder().decode(ItemsWrapper<Item>.self, from: data)

            // Convert the list of items into a dictionary
            entitiesData = decoded.Items
        } catch {
            // Handle any errors that occur during decoding
            print("Failed to parse JSON string: \(error)")
        }
    }
    
    // Attempt to convert the string to Data
    if let data = entityTypesString.data(using: .utf8) {
        do {
            // Decode the JSON data into the ItemsWrapper structure
            let decoded = try JSONDecoder().decode(ItemsWrapper<String>.self, from: data)

            // Convert the list of items into a dictionary
            entityTypesData = decoded.Items
        } catch {
            // Handle any errors that occur during decoding
            print("Failed to parse JSON string: \(error)")
        }
    }
}

@_cdecl("SetSwiftUIPanels")
public func setSwiftUIPanels(_ cPanels: UnsafePointer<CChar>) {
    let panelsString = String(cString: cPanels)
    
    // Attempt to convert the string to Data
    guard let jsonData = panelsString.data(using: .utf8) else { return }
    
    do {
        // Decode the JSON data into the ItemsWrapper structure
        let decoded = try JSONDecoder().decode(ItemsWrapper<Item>.self, from: jsonData)
        
        panelsData = decoded.Items
    } catch {
        print("Failed to decode JSON: \(error)")
    }
}

// Declared in C# as: static extern void CloseSwiftUIWindow(string name);
@_cdecl("CloseSwiftUIWindow")
func closeSwiftUIWindow(_ cname: UnsafePointer<CChar>)
{
    let dismissWindow = EnvironmentValues().dismissWindow

    let name = String(cString: cname)
    print("############ CLOSE WINDOW \(name)")
    dismissWindow(id: name)
}


@MainActor
public class ConnectionStatusModel: ObservableObject {
    @Published public var message: StatusMessage?

    public static let shared = ConnectionStatusModel()
    
    private init() {}
    
    public func setMessage(status: Int, text: String, uri: String) {
        message = StatusMessage(status: status, text: text, uri: uri)
    }
    
    public func setMessage(status: Int, text: String) {
        message = StatusMessage(status: status, text: text, uri: "")
    }
}

public func GetHassUrl() -> String {
    return hassUrl
}

public func GetHassPort() -> String {
    return hassPort
}

public func GetHassToken() -> String {
    return hassToken
}

// Struct to hold the status and message
public struct StatusMessage {
    public var status: Int
    public var text: String
    public var uri: String
    
    public init(status: Int, text: String, uri: String) {
        self.status = status
        self.text = text
        self.uri = uri
    }
}

// Struct for the Panels-Overview tab
public struct Item: Codable {
    public let entityId: String
    public let panelId: String
    public let name: String
    public let active: Bool
}

// Struct for unwrapping Jsons
struct ItemsWrapper<T: Codable>: Codable {
    let Items: [T]
}
