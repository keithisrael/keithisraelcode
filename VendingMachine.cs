/****************************************************************************************************************************************************
*             This is a Test Driven Development solution to the Vending Machine problem presented on the                                       *
*                                                                                                                                                   *
*             url link: https://github.com/guyroyse/vending-machine-kata                                                                            *
*                                                                                                                                                  *
*                                                                         Author:      Keith Israel                                                 *
*                                                                         Location:    Des Moines, IA  USA                                          *
*                                                                         Year:        April 2016                                                   *
*                                                                                                                                                   *
*                                                                                                                   All Rights Reserved.            *
*                                                                                                                                                   *
*****************************************************************************************************************************************************/



using System;
using System.Collections.Generic;

namespace VendingMachineApplication
{

    #region main class
    class VendingMachine
    {
        #region properties
        static string[] message = { "INSERT COIN", "MACHINE ERROR", "INVALID COIN", "", "SOLD OUT", "EXACT CHANGE!" };
        static string display = string.Empty;
        static ISerialComm comm = null;
        static ISelection selection = null;
        static ICoin[] coins = null;
        static IProduct[] products = null;
        static double inputSum = 0;
        static string serialPort = "COM1";
        static Dictionary<string, int> items = new Dictionary<string, int>();

        #endregion properties

        static void Main()
        {
            init();

            while (true) // Loop indefinitely
            {
                checkPortComm();
                Console.WriteLine(checkDisplay());         // getDisplay()   --> check comm error codes
                System.Threading.Thread.Sleep(1000);
            }
        }

        #region program methods

        static void init()
        {
            setUpPort();
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
                if (comm.Signal == (int)SerialComm.SignalType.coinSignal)
                {
                    ICoin inputCoin = new Coin((String.Format("inserted coin at: {0}", DateTime.Now)),
                        double.Parse(comm.Data.Substring(1, 4)), double.Parse(comm.Data.Substring(7, 4)), 
                        double.Parse(comm.Data.Substring(13, 4)), coins);    


                    if (inputCoin.IsGenuine)
                    {
                        comm.Data = string.Empty;
                        comm.DataSignal = false;
                        inputSum += inputCoin.Value;
                        display = inputCoin.Value.ToString();
                    }
                    else
                    {
                        display = message[2];
                        rejectCoin(inputCoin);
                        return;
                    }
                }
                //if the data is a new item inserted 
                else if (comm.Signal == (int)SerialComm.SignalType.itemSignal)
                {
                    string inData = comm.Data;
                    string productName = inData.Substring(8, (comm.Data.IndexOf('>') - 1));

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
                            items[product.Name]--;      //reduce the amount of products of that kind by one

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
                else if (comm.Signal == (int)SerialComm.SignalType.selectionReturnSignal)
                {
                    selection = new Selection(true);
                    comm.returnCoins();
                    display = message[0];
                }
                //if the data received concernes the coin reserves
                else if (comm.Signal == (int)SerialComm.SignalType.reservesSignal)
                {
                    string reserveLevel = comm.Data.Substring(11, comm.Data.IndexOf('>') - 1);

                    if (reserveLevel.ToLower().Contains("low")) //low reserves for change
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
        static void rejectCoin(ICoin coin)
        {
            comm.ejectCoin(coin);
        }    

        #endregion program methods 
    }
    #endregion main class  
}