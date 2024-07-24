import UIKit
import Coralogix

class ViewController: UIViewController {

    private var coralogixRum: CoralogixRum
    private var alertMessage: String = "Loading..."

    // MARK: - Initializers

    /// Initializer to configure and initialize CoralogixRum
    init() {
        let options = CoralogixExporterOptions(
            coralogixDomain: CoralogixDomain.US2,
            userContext: nil,
            environment: "PROD",
            application: "CX-Demo-UIKit",
            version: "1",
            publicKey: "",
            ignoreUrls: [],
            ignoreErrors: [],
            customDomainUrl: nil,
            labels: ["test": "example"],
            debug: true
        )
        self.coralogixRum = CoralogixRum(options: options)
        super.init(nibName: nil, bundle: nil)
    }

    /// Required initializer with coder parameter
    required init?(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }

    // MARK: - Lifecycle Methods

    override func viewDidLoad() {
        super.viewDidLoad()
        setupUI()
    }

    // MARK: - UI Setup

    /// Method to set up the user interface
    private func setupUI() {
        view.backgroundColor = UIColor(red: 0.9, green: 1.0, blue: 0.9, alpha: 1.0)
        
        let titleLabel = UILabel()
        titleLabel.text = "Coralogix RUM Demo"
        titleLabel.font = UIFont.systemFont(ofSize: 32)
        titleLabel.textColor = .green
        titleLabel.translatesAutoresizingMaskIntoConstraints = false
        view.addSubview(titleLabel)
        
        let pressMeButton = UIButton(type: .system)
        pressMeButton.setTitle("Press Me", for: .normal)
        pressMeButton.backgroundColor = .green
        pressMeButton.setTitleColor(.white, for: .normal)
        pressMeButton.layer.cornerRadius = 10
        pressMeButton.translatesAutoresizingMaskIntoConstraints = false
        pressMeButton.addTarget(self, action: #selector(fetchData), for: .touchUpInside)
        view.addSubview(pressMeButton)
        
        let causeExceptionButton = UIButton(type: .system)
        causeExceptionButton.setTitle("Cause Exception", for: .normal)
        causeExceptionButton.backgroundColor = .orange
        causeExceptionButton.setTitleColor(.white, for: .normal)
        causeExceptionButton.layer.cornerRadius = 10
        causeExceptionButton.translatesAutoresizingMaskIntoConstraints = false
        causeExceptionButton.addTarget(self, action: #selector(causeException), for: .touchUpInside)
        view.addSubview(causeExceptionButton)
        
        let causeCrashButton = UIButton(type: .system)
        causeCrashButton.setTitle("Cause Crash", for: .normal)
        causeCrashButton.backgroundColor = .red
        causeCrashButton.setTitleColor(.white, for: .normal)
        causeCrashButton.layer.cornerRadius = 10
        causeCrashButton.translatesAutoresizingMaskIntoConstraints = false
        causeCrashButton.addTarget(self, action: #selector(causeCrash), for: .touchUpInside)
        view.addSubview(causeCrashButton)
        
        NSLayoutConstraint.activate([
            titleLabel.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            titleLabel.topAnchor.constraint(equalTo: view.topAnchor, constant: 100),
            
            pressMeButton.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            pressMeButton.topAnchor.constraint(equalTo: titleLabel.bottomAnchor, constant: 20),
            
            causeExceptionButton.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            causeExceptionButton.topAnchor.constraint(equalTo: pressMeButton.bottomAnchor, constant: 20),
            
            causeCrashButton.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            causeCrashButton.topAnchor.constraint(equalTo: causeExceptionButton.bottomAnchor, constant: 20)
        ])
    }

    // MARK: - Actions

    /// Method to fetch data from a URL
    @objc private func fetchData() {
        guard let url = URL(string: "https://jsonplaceholder.typicode.com/posts/1") else {
            showAlert(message: "Invalid URL")
            return
        }

        URLSession.shared.dataTask(with: url) { data, response, error in
            if let error = error {
                DispatchQueue.main.async {
                    self.showAlert(message: "Error: \(error.localizedDescription)")
                }
                return
            }
            
            guard let data = data else {
                DispatchQueue.main.async {
                    self.showAlert(message: "No data")
                }
                return
            }
            
            if let post = try? JSONDecoder().decode(Post.self, from: data) {
                DispatchQueue.main.async {
                    self.alertMessage = "Title: \(post.title)\nBody: \(post.body)"
                    self.showAlert(message: self.alertMessage)
                }
            } else {
                DispatchQueue.main.async {
                    self.showAlert(message: "Failed to decode response")
                }
            }
        }.resume()
    }

    /// Method to intentionally cause an exception
    @objc private func causeException() {
        let array = [1, 2, 3]
        let _ = array[10] // This will cause an out-of-bounds exception
    }

    /// Method to intentionally cause a crash
    @objc private func causeCrash() {
        fatalError("This is a forced crash.")
    }

    // MARK: - Helper Methods

    /// Method to display an alert with a given message
    private func showAlert(message: String) {
        let alert = UIAlertController(title: "Network Request", message: message, preferredStyle: .alert)
        alert.addAction(UIAlertAction(title: "OK", style: .default))
        present(alert, animated: true)
    }
}

// MARK: - Post Model

/// Model to represent a Post from the API
struct Post: Codable {
    let title: String
    let body: String
}
