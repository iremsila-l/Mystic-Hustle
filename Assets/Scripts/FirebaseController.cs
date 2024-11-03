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

//Arayüz ögeleri tanýmlanýr
public class FirebaseController : MonoBehaviour
{
    public GameObject loginPanel, signUpPanel, forgetPasswordPanel, notificationPanel;
    public InputField loginEmail, loginPassword, forgetPassEmail, signUpUserName, signUpEmail, signUpPassword, signUpConfirmPassword;
    public Text notif_Title_Text, notif_Message_Text, profileUserName_Text, profileUserEmail_Text;
    public Toggle rememberMe;

    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    private bool isSignIn;


    //Firebase baðýmlýlýklarý kontrol edilir
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

    //Giriþ paneli açýlýr
    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }

    //Kayýt paneli açýlýr
    public void OpenSignUpPanel()
    {
        loginPanel.SetActive(false);
        signUpPanel.SetActive(true);
        forgetPasswordPanel.SetActive(false);
    }

    //Þifremi unuttum paneli açýlýr
    public void OpenForgetPassPanel()
    {
        loginPanel.SetActive(false);
        signUpPanel.SetActive(false);
        forgetPasswordPanel.SetActive(true);
    }

    //Kullanýcý giriþ yapmaya çalýþýr ve giriþ alanlarý boþ ise kullanýcýya bildirim gönderir
    public void LoginUser()
    {
        if (string.IsNullOrEmpty(loginEmail.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            ShowNotificationMessage("ERROR!", "Fields empty! Please input details in all fields");
            return;
        }
        SignInUser(loginEmail.text, loginPassword.text);
    }

    //Kullanýcý giriþ yapmaya çalýþýr
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

        //Kayýt alanlarý boþ ise kullanýcýya bildirim gönderir
        public void SignUpUser()
    {
        if (string.IsNullOrEmpty(signUpUserName.text) && string.IsNullOrEmpty(signUpEmail.text) && string.IsNullOrEmpty(signUpPassword.text) && string.IsNullOrEmpty(signUpConfirmPassword.text))
        {
            ShowNotificationMessage("ERROR!", "Fields empty! Please input details in all fields");
            return;
        }

        //Þifreler uyuþmuyor ise kullanýcýya bildirim gönderir
        if (signUpPassword.text != signUpConfirmPassword.text)
        {
            ShowNotificationMessage("ERROR!", "Passwords do not match!");
            return;
        }

        //Yeni kullanýcý oluþturur
        CreateUser(signUpEmail.text, signUpPassword.text, signUpUserName.text);
    }

    //Unutulan þifre alaný boþ ise kullanýcýya bildirim gösterir
    public void ForgetPassword()
    {
        if (string.IsNullOrEmpty(forgetPassEmail.text))
        {
            ShowNotificationMessage("ERROR!", "Fields empty! Please input details in all fields");
            return;
        }

        //Þifre sýfýrlama iþlemini gerçekleþtirir
        ForgetPasswordSubmit(forgetPassEmail.text);
    }

    //Bildirim mesajýný gösterir
    private void ShowNotificationMessage(string title, string message)
    {
        notif_Title_Text.text = title;
        notif_Message_Text.text = message;
        notificationPanel.SetActive(true);
    }

    //Bildirim panelini kapatýr
    public void CloseNotificationPanel()
    {
        notif_Title_Text.text = "";
        notif_Message_Text.text = "";
        notificationPanel.SetActive(false);
    }

    //Yeni kullanýcý oluþturur
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

            //Firebase kullanýcýsýný oluþturur
            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            UpdateUserProfile(Username);
        });
    }

    //Firebase'i baþlatýr'
    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    //Kullanýcý oturum durumu deðiþtiðinde bu fonksiyon çaðrýlýr
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {

        //Eðer mevcut kullanýcý deðiþtiyse
        if (auth.CurrentUser != user)
        {
            //Kullanýcý oturum açmýþ mý kontrol edilir
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();

            //Eðer kullanýcý oturum açmýþ deðilse ve önceki kullanýcý varsa
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            //Mevcut kullanýcý güncellenir
            user = auth.CurrentUser;

            //Eðer oturum açýlmýþsa signed in true çevrilir
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                isSignIn = true;
            }
        }
    }

    void OnDestroy()
    {
        //Nesne yok edilirken FirebaseAuth.StateChanged olayý kaldýrýlýr
        auth.StateChanged -= AuthStateChanged;
        //Auth nesnesi null olarak atanýr
        auth = null;
    }

    //Kullanýcý profili oluþturulur ve güncellenir
    void UpdateUserProfile(string UserName)
    {
        //Mevcut kullanýcý bilgileri alýnýr
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

    //AuthError'a göre hata mesajlarý döndürülür
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

    // Þifre sýfýrlama e-postasý gönderilir
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
                // Hata durumunda hata mesajý gösterilir
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

            // Þifre sýfýrlama e-postasý baþarýyla gönderildi mesajý gösterilir
            ShowNotificationMessage("Success", "Password reset email sent successfully!");
        });
    }
}
