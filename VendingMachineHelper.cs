using System;
using System.IO.Ports;

namespace VendingMachineApplication
{
    #region helper classes

    #region SerialComm class

    public interface ISerialComm 
    {
        bool DataSignal { get; set; }
        int Signal { get; set; }
        string Data { get; set; }
        bool Reserves { get; set; }
        bool CheckReserves { get; set; }
        bool Status { get; set; }
        int ErrorCode { get; set; }
        bool CoinEjected { get; set; }
        bool AllCoinsEjected { get; set; }
        bool ProductEjected { get; set; }
        bool BalanceEjected { get; set; }

        void OpenMyPort();
        void serialPortReceivedInput(object sender, SerialDataReceivedEventArgs e);
        void ejectCoin(ICoin coin);
        void ejectProduct(IProduct selectedProduct);
        void returnCoins();
        void ejectBalance(double productPrice, double insertedSum);
        void checkReserves();
    }

    /*
    class to handle serial port communication  (where coins are inserted)
    */
    public class SerialComm : ISerialComm
    {
        #region fields
        private static SerialPort myPort = null;
        private bool serialOpen = false;
        private string commPort, exception = string.Empty;
        protected int errorCode = -1;
        protected bool receivedData = false, reserves = false, chkReserves = false;
        protected string inData = string.Empty;
        protected int signalType = -1;
        protected bool coinEjected = false;
        protected bool allCoinsEjected = false;
        protected bool productEjected = false;
        protected bool balanceEjected = false;

        public enum ErrorCodes
        {
            machineError,
            coinError,
            unKnownError
        };

        public enum SignalType
        {
            coinSignal,
            itemSignal,
            selectionSignal,
            coinReturnSignal,
            reservesSignal,
            error
        };
        #endregion fields

        #region public accessors
        public String Port
        {
            get
            {
                return this.commPort;
            }
            set
            {
                this.commPort = value;
            }
        }
        public int Signal
        {
            get
            {
                return this.signalType;
            }
            set
            {
                this.signalType = value;
            }
        }
        public bool Status
        {
            get
            {
                return this.serialOpen;
            }
            set
            {
                this.serialOpen = value;
            }
        }
        public String Exception
        {
            get
            {
                return this.exception;
            }
            set
            {
                this.exception = value;
            }
        }
        public int ErrorCode
        {
            get
            {
                return this.errorCode;
            }
            set
            {
                this.errorCode = value;
            }
        }
        public string Data
        {
            get
            {
                return this.inData;
            }
            set
            {
                this.inData = value;
            }
        }
        public bool DataSignal
        {
            get
            {
                return this.receivedData;
            }
            set
            {
                this.receivedData = value;
            }
        }
        public bool Reserves
        {
            get
            {
                return this.reserves;
            }
            set
            {
                this.reserves = value;
            }
        }
        public bool CheckReserves
        {
            get
            {
                return this.chkReserves;
            }
            set
            {
                this.chkReserves = value;
            }
        }
        public bool CoinEjected
        {
            get
            {
                return this.coinEjected;
            }
            set
            {
                this.coinEjected = value;
            }
        }
        public bool AllCoinsEjected
        {
            get
            {
                return this.allCoinsEjected;
            }
            set
            {
                this.allCoinsEjected = value;
            }
        }
        public bool ProductEjected
        {
            get
            {
                return this.productEjected;
            }
            set
            {
                this.productEjected = value;
            }
        }

        public bool BalanceEjected
        {
            get
            {
                return this.balanceEjected;
            }
            set
            {
                this.balanceEjected = value;
            }
        }

        #endregion public accessors

        public SerialComm(string portNo)
        {
            serialOpen = false;
            this.commPort = portNo;
            OpenMyPort();
        }

        public void OpenMyPort()
        {
            try
            {
                myPort = new SerialPort(commPort);
                myPort.DataReceived += new SerialDataReceivedEventHandler(serialPortReceivedInput);
                myPort.Open();
                serialOpen = true;
            }
            catch (Exception ex)
            {
                serialOpen = false;
                errorCode = (int)ErrorCodes.machineError;
                exception = String.Format("Error opening my port: {0} with exception {1}", commPort, ex.Message);
            }
        }

        public void serialPortReceivedInput(object sender, SerialDataReceivedEventArgs e)
        {
            /*
            1. get input data
            2. set bool value to true*/
            SerialPort sp = (SerialPort)sender;
            inData = sp.ReadExisting();
            receivedData = true;

            /*
             FOR UNIT TEST PURPOSE ONLY. REMOVE AFTER SUCCESSFUL UNIT TEST
             */
            inData = (string)sender;

            if (inData.ToLower().Contains("data"))
            {
                if ((inData.ToLower()).Contains("coin"))
                    signalType = (int)SignalType.coinSignal;
                if ((inData.ToLower()).Contains("item"))
                    signalType = (int)SignalType.itemSignal;
                else if ((inData.ToLower()).Equals("selection"))
                    signalType = (int)SignalType.selectionSignal;
                else if ((inData.ToLower()).Equals("coinReturn"))
                    signalType = (int)SignalType.coinReturnSignal;
                else if ((inData.ToLower()).Equals("reserves"))
                    signalType = (int)SignalType.reservesSignal;
                else
                {
                    signalType = (int)SignalType.error;
                    this.errorCode = (int)SerialComm.ErrorCodes.coinError;
                }
            }
            else
            {
                signalType = (int)SignalType.error;
                this.errorCode = (int)SerialComm.ErrorCodes.machineError;
            }
        }

        public void ejectCoin(ICoin coin)
        {
            myPort.WriteLine(String.Format("<Eject><{0}><{1}><{2}>", coin.Length, coin.Height, coin.Weight));
            this.coinEjected = true;
        }


        public void ejectProduct(IProduct product)
        {
            myPort.WriteLine(String.Format("<Eject><{0}><{1}>", product.Name, product.Price));
            this.productEjected = true;
        }
        public void returnCoins()
        {
            myPort.WriteLine("<Eject>");
            this.allCoinsEjected = true;
        }

        public void ejectBalance(double productPrice, double insertedSum)
        {
            this.balanceEjected = false;
            double change = insertedSum - productPrice;
            int noOfQuarters = 0, noOfDimes = 0, noOfNickels = 0;

            try
            {
                noOfQuarters = (int)((change / 0.25) - (change % 0.25));
                noOfDimes = (int)(((change % 0.25) / 0.10) - ((change % 0.25) % 0.10));
                noOfNickels = (int)((((change % 0.25) % 0.10) / 0.05) - (((change % 0.25) % 0.10) % 0.05));
            }
            catch (Exception)
            { }

            if (noOfQuarters > 0)
            {
                for (int i = 0; i < noOfQuarters; i++)  // eject a certain number of quarters as change
                {
                    myPort.WriteLine(String.Format("<Eject><{0}><{1}>", noOfQuarters, 0.25));
                }
            }
            if (noOfDimes > 0)
            {
                for (int i = 0; i < noOfDimes; i++)  // eject a certain number of dimes as change
                {
                    myPort.WriteLine(String.Format("<Eject><{0}><{1}>", noOfDimes, 0.10));
                }
            }
            if (noOfNickels > 0)
            {
                for (int i = 0; i < noOfNickels; i++) // eject a certain number of nickels as change
                {
                    myPort.WriteLine(String.Format("<Eject><{0}><{1}>", noOfNickels, 0.05));
                }
            }
            this.balanceEjected = true;
        }

        public void checkReserves()
        {
            myPort.WriteLine("<Reserves>");
            this.chkReserves = true;
        }
    }

    #endregion SerialComm class

    #region coin class

    //coin base class   
    public class Shape
    {
        protected double length;
        protected double height;

        public double Length
        {
            get
            {
                return this.length;
            }
            set
            {
                this.length = value;
            }
        }
        public double Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }

        public Shape(double length, double height)
        {
            this.length = length;
            this.height = height;
        }
    }

    // Interface coin value
    public interface ICoin
    {
        double Value { get; set; }
        bool IsGenuine { get; set; }
        double Length { get; set; }
        double Weight { get; set; }
        double Height { get; set; }        
    }

    // Coin Derived class
    public class Coin : Shape, ICoin
    {
        protected string type = string.Empty;
        protected double size = 0.0;
        protected double weight = 0.0;
        protected double _value;
        protected bool _isGenuine = false;

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
        public double Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
            }
        }
        public double Weight
        {
            get
            {
                return this.weight;
            }
            set
            {
                this.weight = value;
            }
        }
        public double Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
            }
        }
        public bool IsGenuine
        {
            get
            {
                return this._isGenuine;
            }
            set
            {
                this._isGenuine = value;
            }
        }

        public Coin(string type, double length, double height, double weight, ICoin[] validCoins) : base(length, height)
        {
            this.type = type;
            this.weight = weight;
            this.size = 3.14 * (length / 2) * (length / 2) * height;

            if(validCoins != null)
                validateCoin(validCoins);
        }

        void validateCoin(ICoin[] coinTypes)
        {
            _isGenuine = false;

            foreach(Coin coin in coinTypes)
            {
                double sizeLowerLimit = this.size * 0.95;
                double sizeUpperLimit = this.size * 1.05;
                double weightLowerLimit = this.weight * 0.95;
                double weightUpperLimit = this.weight * 1.05;

                if ((Size >= sizeLowerLimit || Size <= sizeUpperLimit)
                    && (Weight >= weightLowerLimit || Weight <= weightUpperLimit))
                    _isGenuine = true;
            }
        }
    }

    #endregion coin class

    #region Product class

    public interface IProduct
    {
        double Price { get; set; }
        string Name { get; set; }
    }

    //abstract product class
    public abstract class AbstractProduct
    {
        protected double price;
        protected string name;
        public abstract double Price
        {
            get;
            set;
        }
        public abstract string Name
        {
            get;
            set;
        }
    }

    public class Product : AbstractProduct, IProduct
    {
        public override double Price
        {
            get
            {
                return price;
            }
            set
            {
                this.price = value;
            }
        }
        public override string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }

        public Product(string productName, double productPrice)
        {
            this.name = productName;
            this.price = productPrice;
        }
    }

    #endregion Product class

    #region Selection class

    public interface ISelection
    {
        bool SoldOut { get; set; }
        bool SeenSoldOut { get; set; }
        string Display { get; set; }
        double Inserted { get; set; }
        double Balance { get; set; }

        void getBalance();
    }

    public class Selection : ISelection
    {
        #region fields

        protected IProduct product;
        //protected double coinSum;
        protected double change;
        protected double inserted;
        protected string display = string.Empty;
        protected bool _returnCoins = false, soldOut = false, _checked = false;

        public enum Value
        {
            Less, Equal, More
        };

        #endregion fields

        #region accessors

        public IProduct Product
        {
            get
            {
                return this.product;
            }
            set
            {
                this.product = value;
            }
        }
        public double Inserted
        {
            get
            {
                return this.inserted;
            }
            set
            {
                this.inserted = value;
            }
        }
        public double Balance
        {
            get
            {
                return this.change;
            }
            set
            {
                this.change = value;
            }
        }
        public string Display
        {
            get
            {
                return this.display;
            }
            set
            {
                this.display = value;
            }
        }
        public bool Return
        {
            get
            {
                return this._returnCoins;
            }
            set
            {
                this._returnCoins = value;
            }
        }
        public bool SoldOut
        {
            get
            {
                return this.soldOut;
            }
            set
            {
                this.soldOut = value;
            }
        }
        public bool SeenSoldOut
        {
            get
            {
                return this._checked;
            }
            set
            {
                this._checked = value;
            }
        }

        #endregion accessors

        public Selection(bool _return)
        {
            this._returnCoins = _return;
        }

        public Selection(IProduct selectedProduct, double sum)
        {
            this.product = selectedProduct;
            this.inserted = sum;
        }

        public void getBalance()
        {
            if (inserted < product.Price)
            {
                inserted = (int)Value.Less;
                display = String.Format("{0}", inserted);
            }
            else if (inserted >= product.Price)
            {
                if (inserted > product.Price)
                    inserted = (int)Value.More;
                else
                    inserted = (int)Value.Equal;

                this.display = "THANK YOU!";
            }
        }
    }

    #endregion Selection class

    #endregion helper classes

}
