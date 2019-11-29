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
        public string entryOffsetText { get; set; }
        public string entryProtectMode1Text { get; set; }
        public string entryProtectMode2Text { get; set; }

        public string labelResponseStatus { get; set; } = "";
        public string labelKey0Status { get; set; } = "";
        public string labelKey1Status { get; set; } = "";

        public string labelResult1Text { get; set; } = "";
        public string labelResult2Text { get; set; } = "";
        public string labelResult2DateText { get; set; } = "";
        public string labelResult3Text { get; set; } = "";
        public string labelResult3DateText { get; set; } = "";

        public bool switchEncryptionIsToggled { get; set; } = false;

        public bool switchDataValidityIsToggled { get; set; } = false;

        public string entryJsonServAddress { get; set; } = "";

        public ICommand OnSetKey1ButtonCommand { protected set; get; }
        public ICommand OnSetKey2ButtonCommand { protected set; get; }
        public ICommand OnWriteKey0ButtonCommand { protected set; get; }
        public ICommand OnWriteKey1ButtonCommand { protected set; get; }
        public ICommand OnAuthenticateTAM1ButtonCommand { protected set; get; }
        public ICommand OnAuthenticateTAM2ButtonCommand { protected set; get; }
        public ICommand OnAuthenticateTAM3ButtonCommand { protected set; get; }

        uint accessPwd;
        int _currentProcess;

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
            OnAuthenticateTAM3ButtonCommand = new Command(OnAuthenticateTAM3ButtonButtonClick);

            entryOffsetText = "0";
            entryProtectMode1Text = "0";
            entryProtectMode2Text = "2";
            entryJsonServAddress = "http://localhost";

            SetEvent(true);
        }

        private void SetEvent(bool enable)
        {
            // Cancel RFID event handler
            BleMvxApplication._reader.rfid.ClearEventHandler();

            if (enable)
            {
                BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
            }
        }

        public override void Resume()
        {
            base.Resume();
            SetEvent(true);
        }

        public override void Suspend()
        {
            SetEvent(false);
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
            entrySelectedKey0 = "0123456789ABCDEF0123456789ABCDEF";
            RaisePropertyChanged(() => entrySelectedKey0);
        }

        void OnSetKey2ButtonButtonClick()
        {
            entrySelectedKey1 = "0123456789ABCDEF0123456789ABCDEF";
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
            _currentProcess = 0;
            labelResponseStatus = "R";
            RaisePropertyChanged(() => labelResponseStatus);

            labelResult1Text = "Reading...";
            RaisePropertyChanged(() => labelResult1Text);

            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x60;

            //BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = "0000" + entryChallenge;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = TAM1Message(0, false, 0, 0, entryChallenge);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }

        void OnAuthenticateTAM2ButtonButtonClick()
        {
            int protMode;

            labelResponseStatus = "R";
            RaisePropertyChanged(() => labelResponseStatus);

            labelResult2Text = "Reading...";
            RaisePropertyChanged(() => labelResult2Text);
            RaisePropertyChanged(() => entryProtectMode1Text);

            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x78;

            //BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = TAM2Message(0, true, 0, 1, entryChallenge, 0, 0, 1, int.Parse(entryProtectMode1Text));

            if (!switchEncryptionIsToggled && !switchDataValidityIsToggled)
            {
                _currentProcess = 1;
                protMode = 0;
            }
            else if (switchEncryptionIsToggled && !switchDataValidityIsToggled)
            {
                _currentProcess = 1;
                protMode = 1;
            }
            else if (!switchEncryptionIsToggled && switchDataValidityIsToggled)
            {
                _currentProcess = 2;
                protMode = 2;
            }
            else // if (switchEncryptionIsToggled && switchDataValidityIsToggled)
            {
                _currentProcess = 2;
                protMode = 3;
            }

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = TAM2Message(0, true, 0, 1, entryChallenge, 0, 0, 1, protMode);
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }

        void OnAuthenticateTAM3ButtonButtonClick()
        {
            _currentProcess = 2;
            labelResponseStatus = "R";
            RaisePropertyChanged(() => labelResponseStatus);

            RaisePropertyChanged(() => entryProtectMode2Text);

            labelResult3Text = "Reading...";
            RaisePropertyChanged(() => labelResult3Text);

            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x78;

            //BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = "2001" + entryChallenge + "00001100";
            //BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = TAM2Message(0, true, 0, 1, entryChallenge, 0, 0, 1, 1) + "00";
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = TAM2Message(0, true, 0, 1, entryChallenge, 0, 0, 1, int.Parse(entryProtectMode2Text));
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }

        void OnAuthenticateTAM2wProtectModeButtonButtonClick()
        {
            _currentProcess = 2;
            labelResponseStatus = "R";
            RaisePropertyChanged(() => labelResponseStatus);

            RaisePropertyChanged(() => entryOffsetText);

            TagSelected();

            BleMvxApplication._reader.rfid.Options.TagAuthenticate.SenRep = CSLibrary.Structures.SENREP.SEND;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.IncRepLen = CSLibrary.Structures.INCREPLEN.INCLUDE;
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Length = 0x78;

            //BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = "2001" + entryChallenge + "00001100";
            //BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = TAM2Message(0, true, 0, 1, entryChallenge, 0, 0, 1, 1) + "00";
            BleMvxApplication._reader.rfid.Options.TagAuthenticate.Message = TAM2Message(0, true, 0, 1, entryChallenge, 0, int.Parse(entryOffsetText), 1, 1);
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_AUTHENTICATE);
        }

        public static byte[] ToByteArray(String hexString)
        {
            byte[] retval = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
                retval[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return retval;
        }

        public static byte [] Decrypt1(string toDecrypt, string key)
        {
            byte[] keyArray = ToByteArray(key);
            byte[] toEncryptArray = ToByteArray(toDecrypt);

            SymmetricAlgorithm crypt = Aes.Create();
            crypt.Key = ToByteArray(key);
            crypt.Mode = CipherMode.ECB;
            crypt.Padding = PaddingMode.None;

            using (MemoryStream memoryStream = new MemoryStream(toEncryptArray))
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, crypt.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    byte[] decryptedBytes = new byte[toEncryptArray.Length];
                    cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);

                    return decryptedBytes;
                }
            }

            return null;
        }

        string TAM1Message(int authMethod, bool customerData, int RFU, int keyId, string challenge)
        {
            string msg = "";
            int value = 0;

            if (authMethod > 3)
                return "";

            if (RFU > 31)
                return "";

            if (keyId > 3)
                return "";

            if (challenge.Length != 20)
                return "";

            value = (authMethod << 14) | (customerData ? 1 : 0) | (RFU << 8) | (keyId);

            msg = value.ToString("X4");

            msg += challenge;

            return msg;
        }

        string TAM2Message (int authMethod, bool customerData, int RFU, int keyId, string challenge, int profile, int offset, int blockCount, int portMode)
        {
            string msg = "";
            int value = 0;

            if (authMethod > 3)
                return "";

            if (RFU > 31)
                return "";

            if (keyId > 3)
                return "";

            if (challenge.Length != 20)
                return "";

            if (profile > 15)
                return "";

            if (offset > 7)
                return "";

            if (blockCount > 15)
                return "";

            if (portMode > 15)
                return "";

            value = (authMethod << 14) | (customerData ? 0x2000 : 0) | (RFU << 8) | (keyId);

            msg = value.ToString("X4");

            msg += challenge;

            value = (profile << 12) | (offset);

            msg += value.ToString("X4");

            value = (blockCount << 4) | (portMode);

            msg += value.ToString("X2");

            return msg;
        }

        static string DecryptStringFromBytes_Aes(string cipherTexts, string Keys)
        {

            byte[] cipherText = ToByteArray(cipherTexts);
            byte[] key = ToByteArray(Keys);



            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = key;
                aesAlg.Mode = CipherMode.ECB;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }


        public static string aesDecrypt(string SourceStr, string CryptoKey)
        {
            string decrypt = "";

            try
            {
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();


                byte[] key = ToByteArray(CryptoKey);
                byte[] dataByteArray = ToByteArray(SourceStr);

                aes.Key = key;
                aes.Mode = CipherMode.ECB;
                aes.KeySize = 128;

                //byte[] dataByteArray = Convert.FromBase64String(SourceStr);
                using (MemoryStream ms = new MemoryStream(dataByteArray))
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(dataByteArray, 0, dataByteArray.Length);
                        cs.FlushFinalBlock();
                        decrypt = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                CSLibrary.Debug.WriteLine(e.Message);
            }
            return decrypt;
        }



        /// <summary>
        /// 字串解密(非對稱式)
        /// </summary>
        /// <param name="Source">解密前字串</param>
        /// <param name="CryptoKey">解密金鑰</param>
        /// <returns>解密後字串</returns>
        public static string aesDecryptBase64(string SourceStr, string CryptoKey)
        {
            string decrypt = "";
            try
            {
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                byte[] key = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(CryptoKey));
                byte[] iv = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(CryptoKey));
                aes.Key = key;
                aes.IV = iv;

                //byte[] dataByteArray = Convert.FromBase64String(SourceStr);
                byte[] dataByteArray = ToByteArray(SourceStr);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(dataByteArray, 0, dataByteArray.Length);
                        cs.FlushFinalBlock();
                        decrypt = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                CSLibrary.Debug.WriteLine(e.Message);
            }
            return decrypt;
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

        // AES CMAC

        byte[] AESEncrypt(byte[] key, byte[] iv, byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();

                    return ms.ToArray();
                }
            }
        }

        byte[] Rol(byte[] b)
        {
            byte[] r = new byte[b.Length];
            byte carry = 0;

            for (int i = b.Length - 1; i >= 0; i--)
            {
                ushort u = (ushort)(b[i] << 1);
                r[i] = (byte)((u & 0xff) + carry);
                carry = (byte)((u & 0xff00) >> 8);
            }

            return r;
        }


        byte[] AESCMAC(string  key, string  data)
        {
            byte[] keyArray = ToByteArray(key);
            byte[] dataArray = ToByteArray(data);

            return AESCMAC(keyArray, dataArray);
        }

        byte[] AESCMAC(byte[] key, byte[] data)
        {
            // SubKey generation
            // step 1, AES-128 with key K is applied to an all-zero input block.
            byte[] L = AESEncrypt(key, new byte[16], new byte[16]);

            // step 2, K1 is derived through the following operation:
            byte[] FirstSubkey = Rol(L); //If the most significant bit of L is equal to 0, K1 is the left-shift of L by 1 bit.
            if ((L[0] & 0x80) == 0x80)
                FirstSubkey[15] ^= 0x87; // Otherwise, K1 is the exclusive-OR of const_Rb and the left-shift of L by 1 bit.

            // step 3, K2 is derived through the following operation:
            byte[] SecondSubkey = Rol(FirstSubkey); // If the most significant bit of K1 is equal to 0, K2 is the left-shift of K1 by 1 bit.
            if ((FirstSubkey[0] & 0x80) == 0x80)
                SecondSubkey[15] ^= 0x87; // Otherwise, K2 is the exclusive-OR of const_Rb and the left-shift of K1 by 1 bit.

            // MAC computing
            if (((data.Length != 0) && (data.Length % 16 == 0)) == true)
            {
                // If the size of the input message block is equal to a positive multiple of the block size (namely, 128 bits),
                // the last block shall be exclusive-OR'ed with K1 before processing
                for (int j = 0; j < FirstSubkey.Length; j++)
                    data[data.Length - 16 + j] ^= FirstSubkey[j];
            }
            else
            {
                // Otherwise, the last block shall be padded with 10^i
                byte[] padding = new byte[16 - data.Length % 16];
                padding[0] = 0x80;


                byte[] newdata = new byte[data.Length + padding.Length];

                Array.Copy(data, newdata, data.Length);
                Array.Copy(padding, 0, newdata, data.Length, padding.Length);

                data = newdata;

                //data = data.Concat<byte>(padding.AsEnumerable()).ToArray();
                //data = data.con   .Concat<byte>(padding.AsEnumerable()).ToArray();

                // and exclusive-OR'ed with K2
                for (int j = 0; j < SecondSubkey.Length; j++)
                    data[data.Length - 16 + j] ^= SecondSubkey[j];
            }

            // The result of the previous process will be the input of the last encryption.
            byte[] encResult = AESEncrypt(key, new byte[16], data);

            byte[] HashValue = new byte[16];
            Array.Copy(encResult, encResult.Length - HashValue.Length, HashValue, 0, HashValue.Length);

            CSLibrary.Debug.WriteLine(HashValue.ToHexString());

            return HashValue;
        }

        // AES CMAC END


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
            InvokeOnMainThread(() =>
            {
            if (e.access == CSLibrary.Constants.TagAccess.AUTHENTICATE)
            {
                if (e.success)
                {
                    /*
                                        var Response = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToString();
                                        CSLibrary.Debug.WriteLine("Response Length : " + Response.Length);
                                        byte [] DecResponse = Decrypt1(Response, entrySelectedKey0);
                                        string result;

                                        entryResponse = DecResponse.ToHexString();
                                        result = CSLibrary.Tools.Hex.ToString(DecResponse, 6);

                                        labelResponseStatus = "Ok";
                                        RaisePropertyChanged(() => entryResponse);
                    */
                    switch (_currentProcess)
                    {
                        case 0:
                            {
                                var Response = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToString();
                                CSLibrary.Debug.WriteLine("Response Length : " + Response.Length);
                                byte[] DecResponse = Decrypt1(Response, entrySelectedKey0);
                                string result;

                                entryResponse = DecResponse.ToHexString();
                                result = CSLibrary.Tools.Hex.ToString(DecResponse, 6);

                                labelResponseStatus = "Ok";
                                RaisePropertyChanged(() => entryResponse);

                                result = CSLibrary.Tools.Hex.ToString(DecResponse, 6, 10);
                                if (result == entryChallenge)
                                    labelResult1Text = "Challenge Match, Success !";
                                else
                                    labelResult1Text = "Challenge NOT match, Fail !";
                                RaisePropertyChanged(() => labelResult1Text);
                            }
                            break;
                        case 1:
                            {
                                var Response = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToString();
                                CSLibrary.Debug.WriteLine("Response Length : " + Response.Length);
                                byte[] DecResponse = null;
                                string result="";

                                try
                                {
                                    DecResponse = Decrypt1(Response, entrySelectedKey0);
                                    entryResponse = DecResponse.ToHexString();
                                    labelResponseStatus = "Ok";
                                    result = CSLibrary.Tools.Hex.ToString(DecResponse, 6, 10);
                                }
                                catch (Exception ex)
                                {
                                }

                                if (result == entryChallenge)
                                {
                                    labelResult2Text = "Challenge Match, Success !";
                                    labelResult2DateText = CSLibrary.Tools.Hex.ToString(DecResponse, 32);
                                }
                                else
                                {
                                    labelResult2Text = "Challenge NOT match, Fail !";
                                    labelResult2DateText = "";
                                }

                                RaisePropertyChanged(() => entryResponse);
                                RaisePropertyChanged(() => labelResponseStatus);
                                RaisePropertyChanged(() => labelResult2Text);
                                RaisePropertyChanged(() => labelResult2DateText);
                            }
                            break;
                        case 2:
                            {
                                var ResponseFull = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToString();
                                entryResponse = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToBytes().ToHexString();
                                string Response = "1";
                                string mac = "2";
                                byte[] DecResponse = null;
                                string result = "";

                                labelResponseStatus = "Ok";

                                try
                                {
                                    Response = ResponseFull.Substring(0, 64);
                                    mac = ResponseFull.Substring(64, 24);
                                    DecResponse = AESCMAC(entrySelectedKey1, Response);
                                    result = CSLibrary.Tools.Hex.ToString(DecResponse, 0, 12);
                                }
                                catch(Exception ex)
                                {
                                }

                                if (result == mac)
                                {
                                    labelResult2Text = "MAC Match, Success !";
                                    labelResult2DateText = ResponseFull.Substring(64);
                                }
                                else
                                {
                                    labelResult2Text = "MAC NOT match, Fail !";
                                    labelResult2DateText = "";
                                }

                                RaisePropertyChanged(() => entryResponse);
                                RaisePropertyChanged(() => labelResponseStatus);
                                RaisePropertyChanged(() => labelResult2Text);
                                RaisePropertyChanged(() => labelResult2DateText);
                            }
                            break;
                        case 3:
                            {
                                var ResponseFull = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToString();
                                entryResponse = BleMvxApplication._reader.rfid.Options.TagAuthenticate.pData.ToBytes().ToHexString();

                                labelResponseStatus = "Ok";
                                RaisePropertyChanged(() => entryResponse);

                                string Response = ResponseFull.Substring(0, 64);
                                string mac = ResponseFull.Substring(64, 24);
                                byte[] DecResponse = AESCMAC(entrySelectedKey1, Response);
                                string result = CSLibrary.Tools.Hex.ToString(DecResponse, 0, 12);
                                if (result == mac)
                                {
                                    labelResult3Text = "MAC Match, Success !";
                                    labelResult3DateText = ResponseFull.Substring(64);
                                }
                                else
                                {
                                    labelResult3Text = "MAC NOT match, Fail !";
                                    labelResult3DateText = "";
                                }
                                RaisePropertyChanged(() => labelResult3Text);
                                RaisePropertyChanged(() => labelResult3DateText);
                            }
                            break;
                    }
                }
                else
                {
                    labelResponseStatus = "E";

                    switch (_currentProcess)
                    {
                        case 0:
                            labelResult1Text = "Read error";
                            RaisePropertyChanged(() => labelResult1Text);
                            break;

                        case 1:
                            labelResult2Text = "Read error";
                            labelResult2DateText = "";
                            RaisePropertyChanged(() => labelResult2Text);
                            RaisePropertyChanged(() => labelResult2DateText);
                            break;

                        case 2:
                            labelResult3Text = "Read error";
                            labelResult3DateText = "";
                            RaisePropertyChanged(() => labelResult3Text);
                            RaisePropertyChanged(() => labelResult3DateText);
                            break;
                    }
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
            });
        }
    }
}
