import SwiftUI
import Coralogix

// The main entry point of the SwiftUI application
@main
struct DemoAppApp: App {
    // A state variable to hold the CoralogixRum instance
    @State private var coralogixRum: CoralogixRum

    // Initializer to configure and initialize CoralogixRum
    init() {
        // Create the options for the Coralogix RUM exporter
        let options = CoralogixExporterOptions(
            coralogixDomain: CoralogixDomain.US2, // Set the Coralogix domain
            userContext: nil,                     // No user context provided
            environment: "PROD",                  // Environment set to production
            application: "DemoApp-iOS-swiftUI",   // Name of the application
            version: "1",                         // Application version
            publicKey: "", // Public key for authentication
            ignoreUrls: [],                       // List of URLs to ignore
            ignoreErrors: [],                     // List of errors to ignore
            customDomainUrl: "",                  // Custom domain URL (if any)
            labels: ["test": "example"],          // Additional labels for the RUM data
            debug: true                          // Debug mode disabled
        )
        
        // Initialize the CoralogixRum instance with the options
        self.coralogixRum = CoralogixRum(options: options)
    }

    // The body of the app, defining the main scene
    var body: some Scene {
        WindowGroup {
            ContentView() // Display the ContentView as the main interface
        }
    }
}
