
using System;
using System.Collections.Generic;

namespace VendingMachineApplication
{

    #region main class
    public class VendingMachine
    {
        #region properties
        public string[] message = { "INSERT COIN", "MACHINE ERROR", "INVALID COIN", "", "SOLD OUT", "EXACT CHANGE!" };
        public string display = string.Empty;
        public static ISerialComm comm = null;
        public static ISelection selection = null;
        public ICoin[] coins = null;
        public IProduct[] products = null;
        public double inputSum = 0;
        static string serialPort = "COM1";
        public Dictionary<string, int> items = new Dictionary<string, int>();
        public string productName = string.Empty;

        #endregion properties

        public VendingMachine()
        {
            this.init();
        }

        static void Main()
        {
            VendingMachine vm = new VendingMachine();

            while (true) // Loop indefinitely
            {
                vm.checkPortComm();
                Console.WriteLine(vm.checkDisplay());         // getDisplay()   --> check comm error codes
                System.Threading.Thread.Sleep(1000);
            }
        }

        #region program methods

        public void init()
        {
            setUpPort();
            setUpCoinTypes();
            setUpProducts();
            display = message[0];
        }

        void setUpProducts()
        {
            products = new Product[3];
            products[0] = new Product("cola", 1.00);
            products[1] = new Product("chips", 0.50);
            products[2] = new Product("candy", 0.65);
        }

        void setUpCoinTypes()
        {
            coins = new Coin[3];
            coins[0] = new Coin("Nickel", 21.21, 1.95, 5.00, null);
            coins[0].Value = 0.05;
            coins[1] = new Coin("Dime", 17.91, 1.35, 2.27, null);
            coins[1].Value = 0.10;
            coins[2] = new Coin("Quarter", 24.26, 1.75, 5.67, null);
            coins[2].Value = 0.25;
        }

        /*
			set up a port that for serial communication
		*/
        void setUpPort()
        {
            comm = new SerialComm(serialPort);
        }

        /*
			communicate with one port
		*/
        void checkPortComm()
        {
            //if there's data coming in from the serial port
            if (comm.DataSignal)
            {
                //if the data is a coin insertion
                if (comm.Signal == (int)SerialComm.SignalType.coinSignal)
                {
                    ICoin inputCoin = new Coin((String.Format("inserted coin at: {0}", DateTime.Now)),
                        double.Parse(comm.Data.Substring(7, 4)), double.Parse(comm.Data.Substring(13, 4)), 
                        double.Parse(comm.Data.Substring(19, 4)), coins);    


                    if (inputCoin.IsGenuine)
                    {
                        comm.Data = string.Empty;
                        comm.DataSignal = false;
                        incrementSum(inputCoin);
                        changeDisplay(inputSum.ToString());
                    }
                    else
                    {
                        rejectCoin(inputCoin);
                        return;
                    }
                }
                //if the data is a new item inserted 
                else if (comm.Signal == (int)SerialComm.SignalType.itemSignal)
                {
                    string inData = comm.Data;
                    productName = inData.Substring(12, (comm.Data.IndexOf('>') - 1));

                    if (items.Count > 0)
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
                else if (comm.Signal == (int)SerialComm.SignalType.selectionSignal)
                {
                    foreach (Product product in products)
                    {
                        if ((comm.Data.ToLower()).Contains(product.Name))
                        {
                            //if item is sold out
                            if (!items.ContainsKey(product.Name) || items[product.Name] == 0)  //if no products of that name or that product count is zer0
                            {
                                selection = new Selection(product, inputSum);
                                selection.SoldOut = true;
                                selection.SeenSoldOut = false;
                                return;
                            }

                            selection = new Selection(product, inputSum);
                            selection.getBalance();
                            display = selection.Display;
                            decrementInventory(product);

                            /*
								1. if coinSum is less than product price; display coin sum
								2. if coinSum is equal to product price; 1. display THANK YOU   2. EJECT PRODUCT
								3. if coinSum is more than product price; 1. display THANK YOU  2. EJECT PRODUCT  3. EJECT BALANCE
							*/
                            if (selection.Inserted >= (int)Selection.Value.Equal)
                            {
                                comm.ejectProduct(product);
                                if (selection.Balance == (int)Selection.Value.More)
                                    comm.ejectBalance(product.Price, inputSum);
                            }
                        }
                        else
                            display = message[1];
                    }
                }
                //if the data is a return-coins selection
                else if (comm.Signal == (int)SerialComm.SignalType.coinReturnSignal)
                {
                    selection = new Selection(true);
                    comm.returnCoins();
                    changeDisplay(message[0]);
                }
                //if the data received concernes the coin reserves
                else if (comm.Signal == (int)SerialComm.SignalType.reservesSignal)
                {
                    string reserveLevel = comm.Data.Substring(17, comm.Data.IndexOf('>') - 1);
                    ascertainReserves(reserveLevel);
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

        public void ascertainReserves(string reserveLevel)
        {
            if (reserveLevel.ToLower().Contains("low")) //low reserves for change
                comm.Reserves = false;
            else
                comm.Reserves = true;
        }

        public void incrementSum(ICoin newCoin)
        {
            if(newCoin.IsGenuine)
                inputSum += newCoin.Value;
        }

        //reduce the amount of products of that kind by one
        public void decrementInventory(IProduct product)
        {
            items[product.Name]--; 
        }

        public void changeDisplay(string str)
        {
            display = str;
        }

        public string checkDisplay()
        {
            if (!comm.Status)
            {
                switch (comm.ErrorCode)
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
            if (selection != null)
            {
                if (selection.SoldOut)
                {
                    //first time it shows 'SOLD OUT', subsequent times shows amount of coins in tray
                    if (!selection.SeenSoldOut)
                    {
                        selection.SeenSoldOut = true;
                        return display = message[4];
                    }
                    else
                    { 
                        if (inputSum > 0.0)
                            display = inputSum.ToString();
                        else
                            display = message[0];
                        return display;
                    }
                }
                else
                {
                    if (inputSum > 0.0)
                        display = inputSum.ToString();
                    else
                        display = message[0];
                }
            }
            if (!comm.Reserves)
                display = message[5];

            return display;
        }

        /*
			reject inserted invalidcoin 
		*/
        public void rejectCoin(ICoin coin)
        {
            changeDisplay(message[2]);
            comm.ejectCoin(coin);
        }    

        #endregion program methods 
    }
    #endregion main class  
}