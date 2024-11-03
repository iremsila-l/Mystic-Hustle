using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
// using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

//Aray�z �geleri tan�mlan�r
public class FirebaseController : MonoBehaviour
{
    public GameObject loginPanel, signUpPanel, forgetPasswordPanel, notificationPanel;
    public InputField loginEmail, loginPassword, forgetPassEmail, signUpUserName, signUpEmail, signUpPassword, signUpConfirmPassword;
    public Text notif_Title_Text, notif_Message_Text, profileUserName_Text, profileUserEmail_Text;
    public Toggle rememberMe;

    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    private bool isSignIn;


    //Firebase ba��ml�l�klar� kontrol edilir
    private void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }

    //Giri� paneli a��l�r
    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }

    //Kay�t paneli a��l�r
    public void OpenSignUpPanel()
    {
        loginPanel.SetActive(false);
        signUpPanel.SetActive(true);
        forgetPasswordPanel.SetActive(false);
    }

    //�ifremi unuttum paneli a��l�r
    public void OpenForgetPassPanel()
    {
        loginPanel.SetActive(false);
        signUpPanel.SetActive(false);
        forgetPasswordPanel.SetActive(true);
    }

    //Kullan�c� giri� yapmaya �al���r ve giri� alanlar� bo� ise kullan�c�ya bildirim g�nderir
    public void LoginUser()
    {
        if (string.IsNullOrEmpty(loginEmail.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            ShowNotificationMessage("ERROR!", "Fields empty! Please input details in all fields");
            return;
        }
        SignInUser(loginEmail.text, loginPassword.text);
    }

    //Kullan�c� giri� yapmaya �al���r
    void SignInUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);

                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        ShowNotificationMessage("Error", GetErrorMessage(errorCode));
                    }
                }
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
            result.User.DisplayName, result.User.UserId);
            SceneManager.LoadScene("Game");
            profileUserName_Text.text = "" + result.User.DisplayName;
            profileUserEmail_Text.text = "" + result.User.Email;
        });
    }

        //Kay�t alanlar� bo� ise kullan�c�ya bildirim g�nderir
        public void SignUpUser()
    {
        if (string.IsNullOrEmpty(signUpUserName.text) && string.IsNullOrEmpty(signUpEmail.text) && string.IsNullOrEmpty(signUpPassword.text) && string.IsNullOrEmpty(signUpConfirmPassword.text))
        {
            ShowNotificationMessage("ERROR!", "Fields empty! Please input details in all fields");
            return;
        }

        //�ifreler uyu�muyor ise kullan�c�ya bildirim g�nderir
        if (signUpPassword.text != signUpConfirmPassword.text)
        {
            ShowNotificationMessage("ERROR!", "Passwords do not match!");
            return;
        }

        //Yeni kullan�c� olu�turur
        CreateUser(signUpEmail.text, signUpPassword.text, signUpUserName.text);
    }

    //Unutulan �ifre alan� bo� ise kullan�c�ya bildirim g�sterir
    public void ForgetPassword()
    {
        if (string.IsNullOrEmpty(forgetPassEmail.text))
        {
            ShowNotificationMessage("ERROR!", "Fields empty! Please input details in all fields");
            return;
        }

        //�ifre s�f�rlama i�lemini ger�ekle�tirir
        ForgetPasswordSubmit(forgetPassEmail.text);
    }

    //Bildirim mesaj�n� g�sterir
    private void ShowNotificationMessage(string title, string message)
    {
        notif_Title_Text.text = title;
        notif_Message_Text.text = message;
        notificationPanel.SetActive(true);
    }

    //Bildirim panelini kapat�r
    public void CloseNotificationPanel()
    {
        notif_Title_Text.text = "";
        notif_Message_Text.text = "";
        notificationPanel.SetActive(false);
    }

    //Yeni kullan�c� olu�turur
    void CreateUser(string email, string password, string Username)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);

                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        ShowNotificationMessage("Error", GetErrorMessage(errorCode));
                    }
                }
                return;
            }

            //Firebase kullan�c�s�n� olu�turur
            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            UpdateUserProfile(Username);
        });
    }

    //Firebase'i ba�lat�r'
    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    //Kullan�c� oturum durumu de�i�ti�inde bu fonksiyon �a�r�l�r
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {

        //E�er mevcut kullan�c� de�i�tiyse
        if (auth.CurrentUser != user)
        {
            //Kullan�c� oturum a�m�� m� kontrol edilir
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();

            //E�er kullan�c� oturum a�m�� de�ilse ve �nceki kullan�c� varsa
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            //Mevcut kullan�c� g�ncellenir
            user = auth.CurrentUser;

            //E�er oturum a��lm��sa signed in true �evrilir
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                isSignIn = true;
            }
        }
    }

    void OnDestroy()
    {
        //Nesne yok edilirken FirebaseAuth.StateChanged olay� kald�r�l�r
        auth.StateChanged -= AuthStateChanged;
        //Auth nesnesi null olarak atan�r
        auth = null;
    }

    //Kullan�c� profili olu�turulur ve g�ncellenir
    void UpdateUserProfile(string UserName)
    {
        //Mevcut kullan�c� bilgileri al�n�r
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            {
                DisplayName = UserName,
                PhotoUrl = new System.Uri("https://example.com/jane-q-user/profile.jpg"),
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");

                ShowNotificationMessage("Alert", "Account Successfully Created");
            });
        }
    }

    //AuthError'a g�re hata mesajlar� d�nd�r�l�r
    private static string GetErrorMessage(AuthError errorCode)
    {
        var message = "";
        switch (errorCode)
        {
            case AuthError.AccountExistsWithDifferentCredentials:
                message = "Account Not Exist";
                break;
            case AuthError.MissingPassword:
                message = "Missing Password";
                break;
            case AuthError.WeakPassword:
                message = "Password So Weak";
                break;
            case AuthError.WrongPassword:
                message = "Wrong Password";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "Your Email Already In Use";
                break;
            case AuthError.InvalidEmail:
                message = "Your Email Invalid";
                break;
            case AuthError.MissingEmail:
                message = "Your Email Missing";
                break;
            default:
                message = "Invalid Error";
                break;
        }
        return message;
    }

    // �ifre s�f�rlama e-postas� g�nderilir
    void ForgetPasswordSubmit(string forgetPasswordEmail)
    {
        auth.SendPasswordResetEmailAsync(forgetPasswordEmail).ContinueWithOnMainThread(task =>
        {

            if (task.IsCanceled)
            {
                Debug.LogError("SendPasswordResetEmailAsync was canceled");
                ShowNotificationMessage("Error", "Password reset email sending was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                // Hata durumunda hata mesaj� g�sterilir
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        ShowNotificationMessage("Error", GetErrorMessage(errorCode));
                        return;
                    }
                }
            }

            // �ifre s�f�rlama e-postas� ba�ar�yla g�nderildi mesaj� g�sterilir
            ShowNotificationMessage("Success", "Password reset email sent successfully!");
        });
    }
}
