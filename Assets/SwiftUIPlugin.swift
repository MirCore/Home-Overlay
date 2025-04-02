import Foundation
import SwiftUI

// Declared in C# as: delegate void CallbackDelegate(string command);
typealias CallbackDelegateType = @convention(c) (UnsafePointer<CChar>, UnsafePointer<CChar>, UnsafePointer<CChar>, UnsafePointer<CChar>) -> Void

var callbackDelegate: CallbackDelegateType? = nil
var hassUrl = "http://homeassistant.local/"
var hassPort = "8123"
var hassToken = ""
var connectionStatus = "Checking connection..."
public var entitiesData: [Item] = []
public var panelsData: [Item] = []

// Declared in C# as: static extern void SetNativeCallback(CallbackDelegate callback);
@_cdecl("SetNativeCallback")
func setNativeCallback(_ delegate: CallbackDelegateType)
{
    print("############ SET NATIVE CALLBACK")
    callbackDelegate = delegate
}

// This is a function for your own use from the enclosing Unity-VisionOS app, to call the delegate
// from your own windows/views (HelloWorldContentView uses this)
public func CallCSharpCallback(_ str: String, _ arg0: String = "", _ arg1: String = "", _ arg2: String = "")
{
    guard let callbackDelegate = callbackDelegate else {
        print("ERROR: Callback delegate is nil")
        return
    }

    // Log the values and confirm UTF-8 encoding
    print("CallCSharpCallback invoked with:")
    print("Command: \(str), arg0: \(arg0), arg1: \(arg1), arg2: \(arg2)")

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

// Declared in C# as: static extern void OpenSwiftUIWindow(string name);
@_cdecl("OpenSwiftUIWindow")
func openSwiftUIWindow(_ cname: UnsafePointer<CChar>)
{
    let openWindow = EnvironmentValues().openWindow

    let name = String(cString: cname)
    print("############ OPEN WINDOW \(name)")
    openWindow(id: name)
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


@MainActor
public class ConnectionStatusModel: ObservableObject {
    @Published public var message: StatusMessage?

    public static let shared = ConnectionStatusModel()
    
    private init() {}
    
    public func setMessage(status: Int, text: String, savedUri: String) {
        message = StatusMessage(status: status, text: text, savedUri: savedUri)
    }
    
    public func setMessage(status: Int, text: String) {
        message = StatusMessage(status: status, text: text, savedUri: "")
    }
}


// Struct to hold the status and message
public struct StatusMessage {
    public var status: Int
    public var text: String
    public var savedUri: String
    
    public init(status: Int, text: String, savedUri: String) {
        self.status = status
        self.text = text
        self.savedUri = savedUri
    }
}

// Struct for the Panels-Overview tab
public struct Item: Codable {
    public let entityId: String
    public let panelId: String
    public let name: String
    public let active: Bool
}

struct ItemsWrapper<T: Codable>: Codable {
    let Items: [T]
}

@_cdecl("SetSwiftUIConnectionStatus")
public func setSwiftUIConnectionStatus(_ status: Int32, _ cMessage: UnsafePointer<CChar>, _ cSavedUri: UnsafePointer<CChar>) {
    let message = String(cString: cMessage)
    let savedUri = String(cString: cSavedUri)
    print("Setting message from Unity: \(status) - \(message)")
    
    Task { @MainActor in
        ConnectionStatusModel.shared.setMessage(status: Int(status), text: message, savedUri: savedUri)
    }
}

@_cdecl("SetSwiftUIHassEntities")
func setSwiftUIHassEntities(_ cEntities: UnsafePointer<CChar>) {
    let entitiesString = String(cString: cEntities)

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

public func GetHassUrl() -> String {
    return hassUrl
}

public func GetHassPort() -> String {
    return hassPort
}

public func GetHassToken() -> String {
    return hassToken
}
