using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HSMiniJSON;

namespace Funplus {

	/// <summary>
	/// The account module enables players to sign into your game with Funplus login.
	/// Basic patterns when using Funplus login are described here:
	/// <list type="number">
	/// <item>When game is launching for the very first time, a manual login is required.</item>
	/// <item>An automatic login is performed everytime after that when game is starting.</item>
	/// <item>If the automatic login in step 2 fails, a manual login is required.</item>
	/// <item>Player will be required to bind his/her account to email or SNS account
	/// if the player keeps using express login, based on security considerations.</item>
	/// <item>A user center can be shown at any time after player has logged in,
	/// to provide additional functions like 'logout' or 'open support center'.</item>
	/// </list>
	/// </summary>
	public class FunplusAccount : MonoBehaviour {
		private FunplusSession session;

		/// <summary>
		/// This interface defines methods which will be called
		/// on specific moment in the life circle of the session.
		/// </summary>
		public interface IDelegate {

			/// <summary>
			/// This method will be called after session is opened. An argument named
			/// <c>isLoggedIn</c> is passed back to indicate whether player has automatically
			/// login or not.
			/// </summary>
			/// <param name="isLoggedIn"><c>true</c> if player has logged in, <c>false</c> otherwise.</param>
			void OnOpenSession (bool isLoggedIn);

			/// <summary>
			/// This method will be called after player logs in.
			/// </summary>
			/// <param name="session">The active session.</param>
			void OnLoginSuccess (FunplusSession session);

			/// <summary>
			/// This method will be called after player fails to login.
			/// </summary>
			/// <param name="error">Error.</param>
			void OnLoginError (FunplusError error);

			/// <summary>
			/// This method will be called after player logs out.
			/// </summary>
			void OnLogout ();

			/// <summary>
			/// This method will be called after player binds his/her account.
			/// </summary>
			/// <param name="session">The active session.</param>
			void OnBindAccountSuccess (FunplusSession session);

			/// <summary>
			/// This method will be called after player fails to bind account.
			/// </summary>
			/// <param name="error">Error.</param>
			void OnBindAccountError (FunplusError error);

			/// <summary>
			/// This method will be called after user center closes without
			/// any operation is performed.
			/// </summary>
			void OnCloseUserCenter ();
		}

		private static FunplusAccount instance;
		private static IDelegate _delegate;

		private BaseAccountWrapper GetWrapper () {
			if (Application.platform == RuntimePlatform.Android) {
				return FunplusAccountAndroid.GetInstance ();
			} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
				return FunplusAccountIOS.GetInstance ();
			} else {
				return FunplusAccountStub.GetInstance ();
			}
		}

		void Awake () {
			instance = this;
		}

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <returns>The instance.</returns>
		public static FunplusAccount GetInstance () {
			return instance;
		}

		/// <summary>
		/// Sets the game object and delegate to the account module.
		/// </summary>
		/// <param name="gameObjectName">Game object name.</param>
		/// <param name="accountDelegate">The delegate.</param>
		public void SetGameObjectAndDelegate (string gameObjectName, IDelegate accountDelegate) {
			if (_delegate == null) {
				_delegate = accountDelegate;
				GetWrapper ().SetGameObject (gameObjectName);
			} else {
				Logger.Log ("{FunplusAccount.SetGameObjectAndDelegate ()} --> Delegate has already been set");
			}
		}

		/// <summary>
		/// Test if player has logged in or not.
		/// </summary>
		/// <returns><c>true</c>, if player has already logged in, <c>false</c> otherwise.</returns>
		public bool IsUserLoggedIn () {
			return (_delegate != null) && GetWrapper ().IsUserLoggedIn ();
		}

		/// <summary>
		/// Opens the session.
		/// </summary>
		public void OpenSession () {
			if (_delegate == null) {
				// set the game object and delegate first!
				Logger.LogError ("{FunplusAccount.OpenSession ()} --> Please call SetGameObjectAndDelegate() first");
			} else {
				Logger.Log ("Try to open session");
				GetWrapper ().OpenSession ();
			}
		}

		/// <summary>
		/// Shows a login window, from which player can choose an account type to login.
		/// </summary>
		public void Login () {
			GetWrapper ().Login ();
		}

		/// <summary>
		/// Signs in the player by using the given account type.
		/// </summary>
		/// <param name="type">The account type.</param>
		public void Login (FunplusAccountType type) {
			GetWrapper ().Login (type);
		}

		/// <summary>
		/// Signs out the player.
		/// </summary>
		public void Logout () {
			GetWrapper ().Logout ();
		}

		/// <summary>
		/// Shows the user center.
		/// </summary>
		public void ShowUserCenter () {
			GetWrapper ().ShowUserCenter ();
		}

		/// <summary>
		/// Shows an account binding window, from which players can choose an account type to bind his/her account.
		/// </summary>
		public void BindAccount () {
			GetWrapper ().BindAccount ();
		}

		/// <summary>
		/// Binds player's account to the given account type.
		/// </summary>
		/// <param name="type">The account type.</param>
		public void BindAccount (FunplusAccountType type) {
			GetWrapper ().BindAccount (type);
		}

		public FunplusSession GetSession () {
			return session;
		}

		//\cond
		#region callbacks
		public void OnGetAvailableAccountTypesSuccess (string message) {
		}

		public void OnGetAvailableAccountTypesError (string message) {
		}
		
		public void OnOpenSession (string message) {
			var dict = Json.Deserialize (message) as Dictionary<string,object>;
			bool isLoggedIn = (bool)dict ["is_logged_in"];
			_delegate.OnOpenSession (isLoggedIn);
		}

		public void OnLoginSuccess (string message) {
			session = FunplusSession.FromMessage (message);
			_delegate.OnLoginSuccess (session);
		}

		public void OnLoginError (string message) {
			_delegate.OnLoginError (FunplusError.FromMessage (message));
		}

		public void OnLogout (string message) {
			_delegate.OnLogout ();
		}

		public void OnBindAccountSuccess (string message) {
			session = FunplusSession.FromMessage (message);
			_delegate.OnBindAccountSuccess (session);
		}

		public void OnBindAccountError (string message) {
			_delegate.OnBindAccountError (FunplusError.FromMessage (message));
		}

		public void OnCloseUserCenter (string message) {
			_delegate.OnCloseUserCenter ();
		}
		#endregion //callbacks
		//\endcond
	}

}