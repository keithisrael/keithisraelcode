/****************************************************************************************************************************************************
*             This is an Object Oriented Programming solution to the Vending Machine problem presented on the                                       *
*                                                                                                                                                   *
*             url link: https://github.com/guyroyse/vending-machine-kata                                                                            *
*                                                                                                                                                   *
*             This is NOT the correct solution to the problem. The CORRECT solution should be a TDD Implementation.                                 *
*                                                                                                                                                   *
*             This solution is just as an OOP skills evaluation                                                                                     *
*                                                                         Author:      Keith Israel                                                 *
*                                                                         Location:    Des Moines, IA  USA                                          *
*                                                                         Year:        April 2016                                                   *
*                                                                                                                                                   *
*                                                                                                                   All Rights Reserved.            *
*                                                                                                                                                   *
*****************************************************************************************************************************************************/



using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Collections;

namespace VendingMachineApp
{
	
    #region container class
    class Program
    {
        #region properties
		static string[] message = {"INSERT COIN", "MACHINE ERROR", "INVALID COIN", "", "SOLD OUT", "EXACT CHANGE!"};
		static string display = string.Empty;
		static bool machineStatus = false;
		static SerialComm comm = null;
		static Selection selection = null;
		static Coin[] coins = null;
		static Product[] products = null;
		static double inputSum = 0;
		static string serialPort = "COM1";
		static Dictionary <string, int> items = new Dictionary<string, int>();
	
		/*
			Incase of multiple serial ports we would have to introduce multi-threading (assign each port a thread for port monitoring)
		*/	
		//static string[] validPorts = null; 
		//static Dictionary<string, bool>  activePorts;
		//static serialComm[] comm;
	
		#endregion properties
    
        static void Main()
        {
		    init();
        
	        while (true) // Loop indefinitely
	        {
			    checkPortComm();
	            //checkPortsComm();
			
			    Console.WriteLine(checkDisplay());         // getDisplay()   --> check comm error codes
			    System.Threading.Thread.Sleep(1000);
	        }
        }
		
        #region program methods
    
		static void init()
		{
			setUpPort();	
			//setUpPorts();	   //For multiple serial ports communication        
        
			setUpCoinTypes();    
			setUpProducts();
			display = message[0];
		}
    
		static void setUpProducts()
		{
			products = new Product[3];
			products[0] = new Product("cola", 1.00);
			products[1] = new Product("chips", 0.50);
			products[2] = new Product("candy", 0.65);
		}
    
		static void setUpCoinTypes()
		{        
			coins = new Coin[3];
			coins[0] = new Coin("Nickel", 21.21, 1.95, 5.00); 
			coins[0].Value = 0.05;
			coins[1] = new Coin("Dime", 17.91, 1.35, 2.27);
			coins[1].Value = 0.10;
			coins[2] = new Coin("Quarter", 24.26, 1.75, 5.67);
			coins[2].Value = 0.25;
		}
    
		/*
			set up a port that for serial communication
		*/
		static void setUpPort()
		{	
			comm = new SerialComm(serialPort);        
		}    
    
		/*
			communicate with one port
		*/
		static void checkPortComm()
		{
			//if there's data coming in from the serial port
			if (comm.DataSignal)
			{			        
				//if the data is a coin insertion
				if(comm.Signal == (int)SerialComm.SignalType.coinSignal)
				{
					Coin inputCoin = new Coin((String.Format("inserted coin at: {0}", DateTime.Now)), 
						double.Parse(comm.Data.Substring(0,4)), double.Parse(comm.Data.Substring(4, 4)), double.Parse(comm.Data.Substring(8, 4)));    //  CHANGE THIS TO REFLECT < >
            
					bool genuineCoin = false;
					foreach (Coin coin in coins)
					{
						if(inputCoin.isGenuine(coin.Size, coin.Weight))
						{
							comm.Data = string.Empty;
							comm.DataSignal = false;
							inputSum += coin.Value;
							genuineCoin = true;
							display = coin.Value.ToString();
							break;
						}
					}
					if(!genuineCoin)
					{
						display = message[2];
						rejectCoin(inputCoin);
						return;
					}
				}
				//if the data is a new item inserted 
				else if(comm.Signal == (int)SerialComm.SignalType.itemSignal)
				{
					string inData = comm.Data;
					string productName = inData.Substring(8,(comm.Data.IndexOf('>')-1));
	            
					if(items.Count > 0)
					{
						var productNames = new List<string>(items.Keys);
						int index = productNames.IndexOf(productName);
						var productCounts = new List<int>(items.Values);
	                
						int productCount = productCounts[index];
						items[productName] = productCount++;
					}
					else	            
						items.Add(productName, 1);
				}
				//if the data is a product selection
				else if(comm.Signal == (int)SerialComm.SignalType.selectionSignal)
				{
					foreach (Product product in products)
					{
						if((comm.Data.ToLower()).Contains(product.Name))	                
						{     
							//if item is sold out
							if(!items.ContainsKey(product.Name) || items[product.Name] == 0)  //if no products of that name or that product count is zer0
							{	
								selection = new Selection(product, inputSum);
								selection.SoldOut = true;
								selection.SeenSoldOut = false;
								return;    
							}
	            
							selection = new Selection(product, inputSum);
							selection.getBalance();
							display = selection.Display;
							items[product.Name]--;      //reduce the amount of products of that kind by one
	                    
							/*
								1. if coinSum is less than product price; display coin sum
								2. if coinSum is equal to product price; 1. display THANK YOU   2. EJECT PRODUCT
								3. if coinSum is more than product price; 1. display THANK YOU  2. EJECT PRODUCT  3. EJECT BALANCE
							*/            
							if(selection.Inserted >= (int)Selection.Value.Equal)
							{
								comm.ejectProduct(product.Name, product.Price);
								if(selection.Balance == (int)Selection.Value.More)
									comm.ejectBalance(product.Price, inputSum);
							}
						}
						else
							display = message[1];
					}
				}
				//if the data is a return-coins selection
				else if(comm.Signal == (int)SerialComm.SignalType.selectionReturnSignal)
				{
					selection = new Selection(true);
					comm.returnCoins();
					display = message[0];
				}
				//if the data received concernes the coin reserves
				else if(comm.Signal == (int)SerialComm.SignalType.reservesSignal)
				{
					string reserveLevel = comm.Data.Substring(11, comm.Data.IndexOf('>')-1);
	            
					if(reserveLevel.ToLower().Contains("low")) //low reserves for change
						comm.Reserves = false;
					else
						comm.Reserves = true;
				}
				//if faulty data coming in 
				else
				{
					comm.Status = false;
					comm.ErrorCode = (int)SerialComm.ErrorCodes.machineError;//display = message[1];
				}
			}
			else
				comm.checkReserves();  //check if we have enough coins for change, else 'EXACT CHANGE' should be displayed'
		}
	
		static string checkDisplay()
		{	    
			if(!comm.Status)
			{
				switch(comm.ErrorCode)
				{
					case (int)SerialComm.ErrorCodes.coinError:
						display = message[2];
						break;
					case (int)SerialComm.ErrorCodes.machineError:                    
						display = message[1];
						break;
					case (int)SerialComm.ErrorCodes.unKnownError:                     
						display = message[1];
						break;
					default:
						break;
				}	        
				return display; 
			}					
			//if a product is selected
			if(selection != null && selection.SoldOut)
			{
				//first time it shows 'SOLD OUT', subsequent times shows amount of coins in tray
				if(!selection.SeenSoldOut)
				{
					selection.SeenSoldOut = true;
					return display = message[4];
				}
				else
					display = message[0];
				if(inputSum > 0.0)
					display = inputSum.ToString();
	            
				return display;
			}
			if(!comm.Reserves)
				display = message[5];
	        
			return display;
		}
	
		/*
			reject inserted invalidcoin 
		*/
		static void rejectCoin(Coin coin)
		{
			comm.ejectCoin(coin);    
		}
    
		#region communication with multiple coin slots    
		/*
			set up ports that for serial communication
			(For multiple serial port communication)
		*/
		/*static void setUpPorts()
		{
			validPorts = SerialPort.GetPortNames();
        
			int i=0;
				do
				{
					comm[i] = new serialComm(validPorts[i]);
					if(!comm[i].Status)
					{
						activePorts.Add(validPorts[i], false);    //this port is inactive                   
						switch(comm[i].ErrorCode)
						{
							case (int)serialComm.ErrorCodes.coinError:
								display = String.Format("{0}{1}", message[2], i + 1);
								break;
							case (int)serialComm.ErrorCodes.machineError:                    
								display = message[1];
								comm[i]= null;
								validPorts = validPorts.Where(w => w != validPorts[i]).ToArray();   //remove that port from list of valid ports array that is throwing an exception
								break;
							case (int)serialComm.ErrorCodes.unKnownError:                    
								display = message[1];
								comm[i]= null;
								validPorts = validPorts.Where(w => w != validPorts[i]).ToArray();   //remove that port from list of valid ports array that is throwing an exception
								break;
							default:
								break;
						}
						if((comm[i].ErrorCode == (int)serialComm.ErrorCodes.machineError) 
							|| ((comm[i].ErrorCode == (int)serialComm.ErrorCodes.unKnownError))
							continue;
					}
					i++;
					activePorts.Add(validPorts[i], true);    // this port is active
				}while (i<=validPorts.Length);
		}*/
    
		/*
			communication with multiple serial ports 
		*/
		/*static void checkPortsComm()
		{
			int portWithError = checkPortStatus();
			if (machineStatus)
					Console.WriteLine(display[0]); 	  
			else
			{
					switch(comm[portWithError].ErrorCode)
					{
						case (int)serialComm.ErrorCodes.coinError:
							display = String.Format("{0}{1}", message[2], portWithError + 1));
							break;
						case (int)serialComm.ErrorCodes.machineError:
							display = message[1];
							break;
						case (int)serialComm.ErrorCodes.unKnownError:
							display = message[1];
							break;
						default:
							break;
					}
			}
		}*/
    
		/*
			check that all ports are functioning
			(For multiple serial port communication)
		*/
		/*static int checkPortStatus()
		{
			machineStatus = false;
			for(int i=0; i<validPorts.Length; i++)
			{
				//if one port is malfunctioning, return that port number
				if(!comm[i].Status)
					return i;
			}
			//if all ports ok
			machineStatus = true;
			return -1;
		}*/
		#endregion communication with multiple coin slots    
    	
		#endregion program methods        
    	
        #region helper classes
    
        #region SerialComm class
    
			/*
			class to handle serial port communication  (where coins are inserted)
		*/   
			class SerialComm
			{
				#region fields
				private static SerialPort myPort=null;
				private bool serialOpen = false;
				private string commPort , exception = string.Empty;
				int errorCode = -1;
				protected bool receivedData = false, lowReserves = false;
				protected string inData = string.Empty;
				protected int signalType;
        
				double inputCoinLength = 0.0, inputCoinHeight = 0.0;
        
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
				selectionReturnSignal,
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
					return this.lowReserves;
				}
				set
				{
					this.lowReserves = value;
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
         
            void serialPortReceivedInput(object sender, SerialDataReceivedEventArgs e)
            {
				/*
				1. get input data
				2. set bool value to true*/
				SerialPort sp = (SerialPort)sender;
				inData = sp.ReadExisting();   
				receivedData = true;
            
				if((inData.ToLower()).Contains("coin"))
					signalType = (int)SignalType.coinSignal;
				if((inData.ToLower()).Contains("item"))
					signalType = (int)SignalType.itemSignal;
				else if((inData.ToLower()).Equals("selection"))
					signalType = (int)SignalType.selectionSignal;
				else if((inData.ToLower()).Equals("selectionReturn"))
					signalType = (int)SignalType.selectionReturnSignal;
				else if((inData.ToLower()).Equals("reserves"))
					signalType = (int)SignalType.reservesSignal;
				else
					signalType = (int)SignalType.error;
			}
        
            public void ejectCoin(Coin coin)
            {
				myPort.WriteLine(String.Format("<Eject><{0}><{1}><{2}>", coin.Length, coin.Height, coin.Weight));
			}
        
        
	        public void ejectProduct(string productName, double productPrice)
	        {
				myPort.WriteLine(String.Format("<Eject><{0}><{1}>", productName, productPrice));
			}
	        public void returnCoins()
	        {
	            myPort.WriteLine("<Eject>");
	        }
	       
	        public void ejectBalance(double productPrice, double insertedSum)
	        {
				double change = insertedSum - productPrice;
				int noOfQuarters = 0, noOfDimes = 0, noOfNickels = 0;
	        
				try
				{
					noOfQuarters = (int)((change / 0.25) - (change % 0.25));
					noOfDimes = (int)(((change % 0.25) / 0.10) - ((change % 0.25) % 0.10));
					noOfNickels = (int)((((change % 0.25) % 0.10) / 0.05) - (((change % 0.25) % 0.10) % 0.05));
				}
				catch(Exception ex)
				{}       
	         
				if(noOfQuarters>0)
				{
					for(int i=0; i<noOfQuarters;i++)  // eject a certain number of quarters as change
					{
						myPort.WriteLine(String.Format("<Eject><{0}><{1}>", noOfQuarters, 0.25));
					}
				}
				if(noOfDimes>0)
				{
					for(int i=0; i<noOfDimes;i++)  // eject a certain number of dimes as change
					{
						myPort.WriteLine(String.Format("<Eject><{0}><{1}>", noOfDimes, 0.10));
					}
				}
				if(noOfNickels>0)
				{
					for(int i=0; i<noOfNickels;i++) // eject a certain number of nickels as change
					{
						myPort.WriteLine(String.Format("<Eject><{0}><{1}>", noOfNickels, 0.05));
					}
				}
			}
	        
	        public void checkReserves()
	        {
	            myPort.WriteLine("<Reserves>");	            
	        }
        }
    
        #endregion SerialComm class
    
        #region coin class
		
		//coin base class   
		class Shape 
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
		public interface IGenuine 
		{
			bool isGenuine(double size, double weight);
		}
   
		// Coin Derived class
		class Coin : Shape, IGenuine
		{
			protected string type = string.Empty;
			protected double size = 0.0;
			protected double weight = 0.0;
			protected double _value;
        
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
        
			public Coin(string type, double length, double height, double weight): base(length, height)
			{
				this.type = type;
				this.weight = weight;
				this.size = 3.14 * (length / 2) * (length / 2) * height;
			}
        
			public bool isGenuine(double size, double weight)
			{
				bool genuine = false;
            
				double sizeLowerLimit = size * 0.95;
				double sizeUpperLimit = size * 1.05;
				double weightLowerLimit = weight * 0.95;
				double weightUpperLimit = weight * 1.05;
            
				if ((Size >= sizeLowerLimit || Size <= sizeUpperLimit) 
					&& (Weight >= weightLowerLimit || Weight <= weightUpperLimit))
					genuine = true;
            
				return genuine;
			}
		}	
   
		#endregion coin class
    
		#region Product class
    
		//abstract product class
		abstract class AbstractProduct
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
    
		class Product : AbstractProduct
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
    
		public interface IBalance
		{
			void getBalance();
		}
    
		class Selection : IBalance
		{
			#region fields
        
			protected Product product;
			protected double coinSum;
			protected double change;
			protected int inserted;
			protected string display = string.Empty; 
			protected bool _returnCoins = false, soldOut = false, _checked = false;
        
			public enum Value 
			{
				Less, Equal, More
			};
        
			#endregion fields
        
			#region accessors
        
			public Product Product
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
			public int Inserted
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
        
			public Selection(Product selectedProduct, double sum)
			{
				this.product = selectedProduct;
				this.coinSum = sum;
			}
        
			public void getBalance()
			{
				if(coinSum < product.Price)
				{
					inserted = (int)Value.Less;
					display = String.Format("{0}", coinSum);
				}
				else if(coinSum >= product.Price)
				{  	              
					if(coinSum > product.Price)
						inserted = (int)Value.More;
					else
						inserted = (int)Value.Equal;
                
					this.display = String.Format("THANK YOU!");
				}
			}
		}
    
		#endregion Selection class
    
		#endregion helper classes
	}	
    #endregion container class  
}