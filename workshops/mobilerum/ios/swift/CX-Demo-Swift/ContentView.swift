import SwiftUI

// The main view of the app
struct ContentView: View {
    // A state variable to control the visibility of the alert
    @State private var showingAlert = false
    // A state variable to store the fetched data
    @State private var alertMessage = "Loading..."

    var body: some View {
        VStack(spacing: 20) {
            // Text displayed above the buttons
            Text("Coralogix RUM Demo")
                .font(.largeTitle) // Set the font size to large
                .foregroundColor(.green) // Set the text color to green
                .padding(.bottom, 20) // Add some padding below the text

            // Button that triggers the network request and alert
            Button(action: {
                fetchData()
            }) {
                // Button label
                Text("Press Me")
                    .padding() // Add padding around the text
                    .background(Color.green) // Set the background color of the button to green
                    .foregroundColor(.white) // Set the text color of the button to white
                    .cornerRadius(10) // Make the button corners rounded
            }
            .alert(isPresented: $showingAlert) {
                Alert(
                    title: Text("Network Request"), // The title of the alert
                    message: Text(alertMessage), // The message of the alert
                    dismissButton: .default(Text("OK")) // The dismiss button of the alert
                )
            }

            // Button that causes an exception
            Button(action: {
                causeException()
            }) {
                Text("Cause Exception")
                    .padding()
                    .background(Color.orange)
                    .foregroundColor(.white)
                    .cornerRadius(10)
            }

            // Button that causes a crash
            Button(action: {
                causeCrash()
            }) {
                Text("Cause Crash")
                    .padding()
                    .background(Color.red)
                    .foregroundColor(.white)
                    .cornerRadius(10)
            }
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity) // Expand to fill the entire screen
        .background(Color(red: 0.9, green: 1.0, blue: 0.9)) // Set the background color to light green
    }

    // Function to fetch data from a public API
    func fetchData() {
        // Define the URL
        guard let url = URL(string: "https://jsonplaceholder.typicode.com/posts/1") else {
            self.alertMessage = "Invalid URL"
            self.showingAlert = true
            return
        }

        // Create a data task
        URLSession.shared.dataTask(with: url) { data, response, error in
            // Handle error
            if let error = error {
                self.alertMessage = "Error: \(error.localizedDescription)"
                DispatchQueue.main.async {
                    self.showingAlert = true
                }
                return
            }

            // Ensure there is data
            guard let data = data else {
                self.alertMessage = "No data"
                DispatchQueue.main.async {
                    self.showingAlert = true
                }
                return
            }

            // Parse the JSON data
            if let post = try? JSONDecoder().decode(Post.self, from: data) {
                self.alertMessage = "Title: \(post.title)\nBody: \(post.body)"
            } else {
                self.alertMessage = "Failed to decode response"
            }

            // Update the state to show the alert
            DispatchQueue.main.async {
                self.showingAlert = true
            }
        }.resume() // Start the data task
    }

    // Function to cause an exception
    func causeException() {
        let array = [1, 2, 3]
        let _ = array[10] // This will cause an array index out of range exception
    }

    // Function to cause a crash
    func causeCrash() {
        fatalError("This is a forced crash.")
    }
}

// Struct to decode the JSON response
struct Post: Codable {
    let title: String
    let body: String
}

// A preview provider to render the view in Xcode's preview pane
#Preview {
    ContentView()
}
