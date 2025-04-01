import Foundation
import SwiftUI

// These methods are exported from Swift with an explicit C-style name using @_cdecl,
// to match what DllImport expects. You will need to do appropriate conversion from
// C-style argument types (including UnsafePointers and other friends) into Swift
// as appropriate.

// SetNativeCallback is called from the SwiftUIDriver MonoBehaviour in OnEnable,
// to give Swift code a way to make calls back into C#. You can use one callback or
// many, as appropriate for your application.
//
// Declared in C# as: delegate void CallbackDelegate(string command);
typealias CallbackDelegateType = @convention(c) (UnsafePointer<CChar>, UnsafePointer<CChar>, UnsafePointer<CChar>, UnsafePointer<CChar>) -> Void

var callbackDelegate: CallbackDelegateType? = nil
var hassUrl = "http://homeassistant.local/"
var hassPort = "8123"
var hassToken = ""
public var entitiesDict: [String:String] = [:]

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

// Define the structures to match the JSON format
struct Item: Codable {
    let key: String
    let value: String
}

struct ItemsWrapper: Codable {
    let Items: [Item]
}

@_cdecl("SetSwiftUIHassEntities")
func setSwiftUIHassEntities(_ cEntities: UnsafePointer<CChar>) {
    // Convert the C string to a Swift string
    let entitiesString = String(cString: cEntities)

    // Attempt to convert the string to Data
    if let data = entitiesString.data(using: .utf8) {
        do {
            // Decode the JSON data into the ItemsWrapper structure
            let itemsWrapper = try JSONDecoder().decode(ItemsWrapper.self, from: data)

            // Convert the list of items into a dictionary
            entitiesDict = Dictionary(uniqueKeysWithValues: itemsWrapper.Items.map { ($0.key, $0.value) })

            // Log the parsed entities
            print("Parsed entities: \(entitiesDict)")
        } catch {
            // Handle any errors that occur during decoding
            print("Failed to parse JSON string: \(error)")
        }
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
