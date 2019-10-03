using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;

using System.Windows.Input;
using Xamarin.Forms;


using Plugin.BLE.Abstractions.Contracts;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Extensions;

using System.Security.Cryptography;
using System.IO;

namespace BLE.Client.ViewModels
{
    public class ViewModelXerxesAuthentication : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }
        public string entryChallenge { get; set; }
        public string entryResponse { get; set; }
        public string entrySelectedKey0 { get; set; }       // 128 bits
        public string entrySelectedKey1 { get; set; }       // 16 bits

        public string labelResponseStatus { get; set; } = "";
        public string labelKey0Status { get; set; } = "";
        public string labelKey1Status { get; set; } = "";

        public string entryJsonServAddress { get; set; } = "";

        

        public ICommand OnSetKey1ButtonCommand { protected set; get; }
        public ICommand OnSetKey2ButtonCommand { protected set; get; }
        public ICommand OnWriteKey0ButtonCommand { protected set; get; }
        public ICommand OnWriteKey1ButtonCommand { protected set; get; }
        public ICommand OnAuthenticateTAM1ButtonCommand { protected set; get; }
        public ICommand OnAuthenticateTAM2ButtonCommand { protected set; get; }

        uint accessPwd;

        enum CURRENTOPERATION
        {
            READKEY0,
            READKEY1,
            WRITEKEY0,
            WRITEKEY1,
            ACTIVEKEY0,
            ACTIVEKEY1,
            UNKNOWN
        }

        CURRENTOPERATION _currentOperation = CURRENTOPERATION.UNKNOWN;

        public ViewModelXerxesAuthentication(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnSetKey1ButtonCommand = new Command(OnSetKey1ButtonButtonClick);
            OnSetKey2ButtonCommand = new Command(OnSetKey2ButtonButtonClick);
            OnWriteKey0ButtonCommand = new Command(OnWriteKey0ButtonButtonClick);
            OnWriteKey1ButtonCommand = new Command(OnWriteKey1ButtonButtonClick);
            OnAuthenticateTAM1ButtonCommand = new Command(OnAuthenticateTAM1ButtonButtonClick);
            OnAuthenticateTAM2ButtonCommand = new Command(OnAuthenticateTAM2ButtonButtonClick);

            entryJsonServAddress = "http://localhost";
        }

        public override void Resume()
        {
            base.Resume();
            //BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
        }

        public override void Suspend()
        {
            BleMvxApplication._reader.rfid.OnAccessCompleted -= new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
            base.Suspend();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            entrySelectedEPC = BleMvxApplication._SELECT_EPC;
            entrySelectedPWD = "00000000";
            entryChallenge = "FD5D8048F48DD09AAD22";
            entrySelectedKey0 = "";
            entrySelectedKey1 = "";

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
            RaisePropertyChanged(() => entryChallenge);
            RaisePropertyChanged(() => entrySelectedKey0);
            RaisePropertyChanged(() => entrySelectedKey1);

            BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);

            BleMvxApplication._reader.rfid.SetPowerLevel((uint)BleMvxApplication._config.RFID_Power);
        }

        void OnRandomKeyButtonButtonClick()
        {
            Random rnd = new Random();

            entrySelectedKey0 = "";
            entrySelectedKey1 = "";

            for (int cnt = 0; cnt < 8; cnt++)
            {
                entrySelectedKey0 += rnd.Next(0, 65535).ToString("X4");
                entrySelectedKey1 += rnd.Next(0, 65535).ToString("X4");
            }

            RaisePropertyChanged(() => entrySelectedKey0);
            RaisePropertyChanged(() => entrySelectedKey1);
        }

        void OnSetKey1ButtonButtonClick()
        {
            entrySelectedKey0 = "00000000000000000000000000000000";
            RaisePropertyChanged(() => entrySelectedKey0);
        }

        void OnSetKey2ButtonButtonClick()
        {
            entrySelectedKey1 = "00000000000000000000000000000000";
            RaisePropertyChanged(() => entrySelectedKey1);
        }

        void OnWriteKey0ButtonButtonClick()
        {
            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            TagSelected();
            WriteKey0();
        }

        void OnWriteKey1ButtonButtonClick()
        {
            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            TagSelected();
            WriteKey1();
        }

        void OnAuthenticateTAM1ButtonButtonClick()
        {
            labelResponseStatus = "R";
            RaisePropertyChanged(() => labelResponseStatus);

            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x60;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = "0000" + entryChallenge;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }

        void OnAuthenticateTAM2ButtonButtonClick()
        {
            labelResponseStatus = "R";
            RaisePropertyChanged(() => labelResponseStatus);

            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x78;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = "2001" + entryChallenge + "00001100";
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }

        private byte[] CreateKey(string password, int keyBytes = 32)
        {
            byte[] salt = new byte[] { 80, 70, 60, 50, 40, 30, 20, 10 };
            int iterations = 300;
            var keyGenerator = new Rfc2898DeriveBytes(password, salt, iterations);
            return keyGenerator.GetBytes(keyBytes);
        }

        public string Decrypt(string encryptedValue, string encryptionKey)
        {
            string iv = encryptedValue.Substring(encryptedValue.IndexOf(';') + 1, encryptedValue.Length - encryptedValue.IndexOf(';') - 1);
            encryptedValue = encryptedValue.Substring(0, encryptedValue.IndexOf(';'));

            return AesDecryptStringFromBytes(Convert.FromBase64String(encryptedValue), CreateKey(encryptionKey), Convert.FromBase64String(iv));
        }

        private string AesDecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException($"{nameof(cipherText)}");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException($"{nameof(key)}");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException($"{nameof(iv)}");

            string plaintext = null;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream memoryStream = new MemoryStream(cipherText))
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream))
                    plaintext = streamReader.ReadToEnd();

            }
            return plaintext;
        }

        void TagSelected()
        {
            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(entrySelectedEPC);
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);
        }

        void WriteKey0 ()
        {
            RaisePropertyChanged(() => entrySelectedKey0);
            if (entrySelectedKey0.Length != 32)
            {
                _userDialogs.Alert("Key 0 Error, please input 128bit (32 hex)");
                return;
            }

            _currentOperation = CURRENTOPERATION.WRITEKEY0;

            labelKey0Status = "W";
            RaisePropertyChanged(() => labelKey0Status);

            BleMvxApplication._reader.rfid.Options.TagWrite.bank = CSLibrary.Constants.MemoryBank.RESERVED;
            BleMvxApplication._reader.rfid.Options.TagWrite.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWrite.offset = 0x10; // m_writeAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagWrite.count = 8; // m_writeAllBank.WordUser;
            BleMvxApplication._reader.rfid.Options.TagWrite.pData = CSLibrary.Tools.Hex.ToUshorts(entrySelectedKey0);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE);
        }

        void WriteKey1 ()
        {
            RaisePropertyChanged(() => entrySelectedKey1);
            if (entrySelectedKey1.Length != 32)
            {
                _userDialogs.Alert("Key 1 Error, please input 128bit (32 hex)");
                return;
            }

            _currentOperation = CURRENTOPERATION.WRITEKEY1;

            labelKey1Status = "W";
            RaisePropertyChanged(() => labelKey1Status);

            BleMvxApplication._reader.rfid.Options.TagWrite.bank = CSLibrary.Constants.MemoryBank.RESERVED;
            BleMvxApplication._reader.rfid.Options.TagWrite.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWrite.offset = 0x18; // m_writeAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagWrite.count = 8; // m_writeAllBank.WordUser;
            BleMvxApplication._reader.rfid.Options.TagWrite.pData = CSLibrary.Tools.Hex.ToUshorts(entrySelectedKey1);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            if (e.access == CSLibrary.Constants.TagAccess.AUTHENTICATE)
            {
                if (e.success)
                {
                    entryResponse = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToString();
                    labelResponseStatus = "Ok";
                    RaisePropertyChanged(() => entryResponse);

                    var a = Decrypt(entryResponse, entrySelectedKey0);

                    CSLibrary.Debug.WriteLine(a);
                }
                else
                {
                    labelResponseStatus = "E";
                }
                RaisePropertyChanged(() => labelResponseStatus);
            }
            else if (e.access == CSLibrary.Constants.TagAccess.UNTRACEABLE)
            {
                if (e.success)
                {
                    _userDialogs.Alert("UNTRACEABLE command success!");
                }
                else
                {
                    _userDialogs.Alert("UNTRACEABLE command fail!!!");
                }
            }
            else if (e.access == CSLibrary.Constants.TagAccess.READ)
            {
                switch (e.bank)
                {
                    case CSLibrary.Constants.Bank.USER:
                        if (e.success)
                        {
                            switch (_currentOperation)
                            {
                                case CURRENTOPERATION.READKEY0:
                                    entrySelectedKey0 = BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToString();
                                    labelKey0Status = "O";
                                    RaisePropertyChanged(() => entrySelectedKey0);
                                    RaisePropertyChanged(() => labelKey0Status);
                                    break;

                                case CURRENTOPERATION.READKEY1:
                                    entrySelectedKey1 = BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToString();
                                    labelKey1Status = "O";
                                    RaisePropertyChanged(() => entrySelectedKey1);
                                    RaisePropertyChanged(() => labelKey1Status);
                                    break;
                            }
                        }
                        else
                        {
                            switch (_currentOperation)
                            {
                                case CURRENTOPERATION.READKEY0:
                                    labelKey0Status = "E";
                                    RaisePropertyChanged(() => labelKey0Status);
                                    break;

                                case CURRENTOPERATION.READKEY1:
                                    labelKey1Status = "E";
                                    RaisePropertyChanged(() => labelKey1Status);
                                    break;
                            }
                        }

                        break;
                }
            }
            else if (e.access == CSLibrary.Constants.TagAccess.WRITE)
            {
                switch (e.bank)
                {
                    case CSLibrary.Constants.Bank.UNTRACEABLE:
                        if (e.success)
                        {
                            switch (_currentOperation)
                            {
                                case CURRENTOPERATION.ACTIVEKEY0:
                                    labelKey0Status = "O";
                                    RaisePropertyChanged(() => labelKey0Status);
                                    break;

                                case CURRENTOPERATION.ACTIVEKEY1:
                                    labelKey1Status = "O";
                                    RaisePropertyChanged(() => labelKey1Status);
                                    break;
                            }
                        }
                        else
                        {
                            switch (_currentOperation)
                            {
                                case CURRENTOPERATION.ACTIVEKEY0:
                                    labelKey0Status = "E";
                                    RaisePropertyChanged(() => labelKey0Status);
                                    break;

                                case CURRENTOPERATION.ACTIVEKEY1:
                                    labelKey1Status = "E";
                                    RaisePropertyChanged(() => labelKey1Status);
                                    break;
                            }
                        }
                        break;

                    case CSLibrary.Constants.Bank.SPECIFIC:
                        if (e.success)
                        {
                            switch (_currentOperation)
                            {
                                case CURRENTOPERATION.WRITEKEY0:
                                case CURRENTOPERATION.ACTIVEKEY0:
                                    labelKey0Status = "O";
                                    RaisePropertyChanged(() => labelKey0Status);
                                    break;

                                case CURRENTOPERATION.WRITEKEY1:
                                case CURRENTOPERATION.ACTIVEKEY1:
                                    labelKey1Status = "O";
                                    RaisePropertyChanged(() => labelKey1Status);
                                    break;
                            }
                        }
                        else
                        {
                            switch (_currentOperation)
                            {
                                case CURRENTOPERATION.WRITEKEY0:
                                case CURRENTOPERATION.ACTIVEKEY0:
                                    labelKey0Status = "E";
                                    RaisePropertyChanged(() => labelKey0Status);
                                    break;

                                case CURRENTOPERATION.WRITEKEY1:
                                case CURRENTOPERATION.ACTIVEKEY1:
                                    labelKey1Status = "E";
                                    RaisePropertyChanged(() => labelKey1Status);
                                    break;
                            }
                        }

                        break;
                }
            }

        }
    }
}
