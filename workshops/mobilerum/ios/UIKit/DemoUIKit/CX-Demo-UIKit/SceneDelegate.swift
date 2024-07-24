import UIKit

class SceneDelegate: UIResponder, UIWindowSceneDelegate {

    var window: UIWindow?

    /// This method is called when the scene is about to connect to the session. Use this method to configure and attach the UIWindow to the provided UIWindowScene.
    func scene(_ scene: UIScene, willConnectTo session: UISceneSession, options connectionOptions: UIScene.ConnectionOptions) {
        guard let windowScene = (scene as? UIWindowScene) else { return }

        window = UIWindow(windowScene: windowScene)
        window?.rootViewController = ViewController()
        window?.makeKeyAndVisible()
    }

    /// This method is called when the scene is being released by the system. Use this method to release any resources associated with this scene.
    func sceneDidDisconnect(_ scene: UIScene) {
    }

    /// This method is called when the scene has moved from an inactive state to an active state. Use this method to restart any tasks that were paused.
    func sceneDidBecomeActive(_ scene: UIScene) {
    }

    /// This method is called when the scene will move from an active state to an inactive state. This may occur due to temporary interruptions (e.g., incoming phone call).
    func sceneWillResignActive(_ scene: UIScene) {
    }

    /// This method is called as the scene transitions from the background to the foreground. Use this method to undo the changes made on entering the background.
    func sceneWillEnterForeground(_ scene: UIScene) {
    }

    /// This method is called as the scene transitions from the foreground to the background. Use this method to save data, release shared resources, and store enough state information to restore the scene.
    func sceneDidEnterBackground(_ scene: UIScene) {
    }
}
