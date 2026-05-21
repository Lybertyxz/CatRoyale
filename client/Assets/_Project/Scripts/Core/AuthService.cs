using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using CatRoyale.Network;

namespace CatRoyale.Core
{
    public class AuthService
    {
        private FirebaseAuth _auth;
        private FirebaseUser _currentUser;

        public bool IsLoggedIn => _currentUser != null;
        public string UserID => _currentUser?.UserId;
        public string DisplayName => _currentUser?.DisplayName;

        public static async Task<AuthService> Initialize()
        {
            var instance = new AuthService();
            await instance.InitializeFirebase();
            ServiceLocator.Register(instance);
            return instance;
        }

        private async Task InitializeFirebase()
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                _auth = FirebaseAuth.DefaultInstance;
                _currentUser = _auth.CurrentUser;
                Debug.Log("[AuthService] Firebase initialized.");
            }
            else
            {
                Debug.LogError($"[AuthService] Firebase dependency error: {dependencyStatus}");
            }
        }

        // Login avec Google
        public async Task<bool> LoginWithGoogle(string googleIdToken, string accessToken)
        {
            try
            {
                var credential = GoogleAuthProvider.GetCredential(googleIdToken, accessToken);
                _currentUser = await _auth.SignInWithCredentialAsync(credential);

                var firebaseToken = await _currentUser.TokenAsync(false);
                await RegisterWithBackend(firebaseToken);

                Debug.Log($"[AuthService] Logged in as: {_currentUser.DisplayName}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AuthService] Login failed: {e.Message}");
                return false;
            }
        }

        // Récupère le token Firebase et enregistre avec le backend
        public async Task<bool> RefreshAndRegister()
        {
            if (_currentUser == null) return false;

            try
            {
                var firebaseToken = await _currentUser.TokenAsync(true);
                await RegisterWithBackend(firebaseToken);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AuthService] Refresh failed: {e.Message}");
                return false;
            }
        }

        private async Task RegisterWithBackend(string firebaseToken)
        {
            var api = ServiceLocator.Get<ApiService>();
            if (api == null) return;

            api.SetAuthToken(firebaseToken);
            var user = await api.Login(firebaseToken);

            if (user != null)
            {
                Debug.Log($"[AuthService] Backend registered: {user.Username}");
            }
        }

        public void Logout()
        {
            _auth?.SignOut();
            _currentUser = null;
            Debug.Log("[AuthService] Logged out.");
        }
    }
}