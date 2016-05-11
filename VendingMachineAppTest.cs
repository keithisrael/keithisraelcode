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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using VendingMachineApplication;
using System.IO.Ports;

namespace VendingMachineApp.Test
{
    #region VendingMachineApp unit tests

    [TestFixture]
    public class VendingMachineTest
    {
        VendingMachine vm;
        ISerialComm comm;
        ISelection selection;
        IProduct product;

        /// <summary>
        /// This runs only once at the beginning of all tests and is used for all tests in the
        /// class.
        /// </summary>
        [OneTimeSetUp]
        public void InitialSetup()
        {
            vm = new VendingMachine();
            comm = new SerialComm("COM1");       //Assumption: Vending Machine using serial port "COM1" for Input/Output
            vm.init();
        }

        /// <summary>
        /// This runs only once at the end of all tests as destructor to clear allocated memory
        /// class.
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            this.vm = null;
            this.comm = null;
        }

        #region check communication on serial port

        #region serial port input data unit tests

        //Test if new serial port input data (coin insertion, selection or coin-return request) results in a data signal (bool - true)
        [Test]
        void TestIfNewUserInput()
        {
            comm.serialPortReceivedInput("<data>", null);
            Assert.IsTrue(comm.DataSignal);
        }

        [Test] //Test faulty/invalid data received on serial port 
        void TestFaultyInvalidDataSerialPortReceived()
        {
            comm.serialPortReceivedInput("<@#*&^><345^&*>", null);
            Assert.AreEqual(5, comm.Signal, "Failed to correctly identify that data received on serial port is faulty.");
        }

        #endregion serial port data unit tests

        #region coin unit tests

        [Test] //if the data received on serial port is a coin insertion
        void TestCoinSignal()
        {
            comm.serialPortReceivedInput("<data><coin>", null);
            Assert.AreEqual(0, comm.Signal);
        }

        [Test] //Test if input coin is not genuine, it results in a bool value IsGenuine that is false
        void TestInputCoinIsGenuine()
        {
            ICoin newCoin = new Coin(null, 32.50, 23.45, 05.50, vm.coins);
            Assert.IsFalse(newCoin.IsGenuine);
        }

        #region different denomination coin unit tests

        [Test] //Test if input coin is a nickel, it results in a bool value IsGenuine that is true
        void TestIfNewCoinIsNickel()
        {
            Coin newCoin = new Coin("Nickel", 21.21, 1.95, 5.00, vm.coins);
            Assert.IsTrue(newCoin.IsGenuine, "The inserted coin is not a valid US denomination coin");
        }

        [Test] //Test if input coin is a dime, it results in a bool value IsGenuine that is true
        void TestIfNewCoinIsDime()
        {
            Coin newCoin = new Coin("Dime", 17.91, 1.35, 2.27, vm.coins);
            Assert.IsTrue(newCoin.IsGenuine, "The inserted coin is not a valid US denomination coin");
        }

        [Test] //Test if input coin is a quarter, it results in a bool value IsGenuine that is true
        void TestIfNewCoinIsQuarter()
        {
            Coin newCoin = new Coin("Quarter", 24.26, 1.75, 5.67, vm.coins);
            Assert.IsTrue(newCoin.IsGenuine, "The inserted coin is not a valid US denomination coin");
        }

        #endregion different denomination coin unit tests

        #region input sum unit tests
        [Test] //Test that the input sum is incremented after a valid coin is inserted
        void TestincrementSum()
        {
            vm.inputSum = 0.05;
            Coin newCoin = new Coin("Quarter", 24.26, 1.75, 5.67, vm.coins);
            newCoin.Value = 0.25;
            vm.incrementSum(newCoin);
            Assert.AreEqual(0.30, vm.inputSum, "Increment sum failed." );
        }

        [Test] //Test that the input sum is not incremented after a invalid coin is inserted
        void TestNotIncrementSum()
        {
            vm.inputSum = 0.05;
            Coin newCoin = new Coin(null, 4.21, 1.00, 0.67, vm.coins);
            newCoin.Value = 0.25;
            Assert.AreNotEqual(0.30, vm.inputSum, "The input sum was incremented. Please check new coin validation!");
        }
        #endregion input sum unit tests

        #region coin display unit tests

        [Test] //Test if the display shows the new input sum
        void TestIfDisplayShowsIncrementedSumIfValidCoinInserted()
        {
            vm.inputSum = 0;
            Coin newCoin = new Coin("Nickel", 21.21, 1.95, 5.00, vm.coins);
            newCoin.Value = 0.05;
            vm.incrementSum(newCoin);
            vm.changeDisplay(vm.inputSum.ToString());
            Assert.AreEqual(vm.inputSum.ToString(), vm.display, "The display failed to show incremented input sum.");
        }

        [Test] //Test if display shows 'invalid coin' if an invalid coin is inserted
        void TestIfDisplayShowsInvalidCoinIfInvalidCoinInserted()
        {
            vm.inputSum = 0;
            Coin newCoin = new Coin(null, 1.21, 1.05, 3.00, vm.coins);
            newCoin.Value = 0.05;
            vm.incrementSum(newCoin);
            vm.rejectCoin(newCoin);
            Assert.AreEqual(vm.message[2], vm.display, "The display failed to show 'invalid coin'.");
        }

        #endregion coin display unit tests

        #region reject coin unit tests

        [Test] //Test if eject coin command is sent to serial port if an invalid coin is inserted
        void TestRejectInvalidCoin()
        {
            Coin newCoin = new Coin(null, 1.21, 1.05, 3.10, vm.coins);
            vm.rejectCoin(newCoin);
            Assert.IsTrue(comm.CoinEjected, "The coin was not ejected.");
        }

        #endregion reject coin unit tests

        #endregion coin unit tests

        #region new item inserted unit tests

        [Test] //if the data received on serial port is an item selection
        void TestNewItemInsertedSignal()
        {
            comm.serialPortReceivedInput("<data><item>", null);
            Assert.AreEqual(1, comm.Signal);
        }

        [Test] //Test if product name of a new item inserted is received from the serial port
        void TestIfNewItemNameReceivedFromSerialPort()
        {
            comm.serialPortReceivedInput("<data><item><cola>", null);
            Assert.IsNotEmpty(vm.productName, "No item product name received from serial port.");
        }

        #region product name of new item inserted unit tests

        [Test] //Test if product name of a new cola item inserted is received from the serial port
        void TestIfColaItemNameReceivedFromSerialPort()
        {
            comm.serialPortReceivedInput("<data><item><cola>", null);
            string cola = "cola";
            Assert.AreEqual(vm.productName, cola, "No cola product name received from serial port.");
        }
        [Test] //Test if product name of a new chips item inserted is received from the serial port
        void TestIfChipsItemNameReceivedFromSerialPort()
        {
            comm.serialPortReceivedInput("<data><item><chips>", null);
            string chips = "chips";
            Assert.AreEqual(vm.productName, chips, "No chips product name received from serial port.");
        }
        [Test] //Test if product name of a new candy item inserted is received from the serial port
        void TestIfCandyItemNameReceivedFromSerialPort()
        {
            comm.serialPortReceivedInput("<data><item><candy>", null);
            string candy = "candy";
            Assert.AreEqual(vm.productName, candy, "No candy product name received from serial port.");
        }

        #endregion product name of new item inserted unit tests

        #region product count of new item inserted unit tests

        [Test] //Test if product count of cola items incremented if new cola item is inserted
        void TestIfColaCountIncremented()
        {
            vm.items = new Dictionary<string, int>();
            comm.serialPortReceivedInput("<data><item><cola>", null);
            vm.items.Add("cola", 1);
            Assert.AreEqual(1, vm.items["cola"], "Cola was not added to the product inventory");
        }

        [Test] //Test if product count of chips items incremented if new chips item is inserted
        void TestIfChipsCountIncremented()
        {
            vm.items = new Dictionary<string, int>();
            comm.serialPortReceivedInput("<data><item><chips>", null);
            vm.items.Add("chips", 1);
            Assert.AreEqual(1, vm.items["chips"], "Chips was not added to the product inventory");
        }

        [Test] //Test if product count of candy items incremented if new candy item is inserted
        void TestIfCandyCountIncremented()
        {
            vm.items = new Dictionary<string, int>();
            comm.serialPortReceivedInput("<data><item><candy>", null);
            vm.items.Add("candy", 1);
            Assert.AreEqual(1, vm.items["candy"], "Candy was not added to the product inventory");
        }

        #endregion product count of new item inserted unit tests

        #endregion new item inserted unit tests

        #region product selection unit tests

        [Test] //if the data received on serial port is a product selection
        void TestProductSelectionSignal()
        {
            comm.serialPortReceivedInput("<data><selection>", null);
            Assert.AreEqual(2, comm.Signal);
        }

        #region product selection product name unit tests

        [Test] //Test if product selection is cola
        void TestIfProductSelectionCola()
        {
            string serialPortInputData = "<data><selection><cola>";
            comm.serialPortReceivedInput(serialPortInputData, null);
            product = new Product(serialPortInputData.Substring(18, 4), 0.0);
            Assert.AreEqual(product.Name, vm.products, String.Format("Product {0} not recognized.", product.Name));
        }

        [Test] //Test if product selection is chips
        void TestIfProductSelectionChips()
        {
            string serialPortInputData = "<data><selection><chips>";
            comm.serialPortReceivedInput(serialPortInputData, null);
            product = new Product(serialPortInputData.Substring(18, 5), 0.0);
            Assert.AreEqual(product.Name, vm.products, String.Format("Product {0} not recognized.", product.Name));
        }

        [Test] //Test if product selection is candy
        void TestIfProductSelectionCandy()
        {
            string serialPortInputData = "<data><selection><candy>";
            comm.serialPortReceivedInput(serialPortInputData, null);
            product = new Product(serialPortInputData.Substring(18, 5), 0.0);
            Assert.AreEqual(product.Name, vm.products, String.Format("Product {0} not recognized.", product.Name));
        }

        #endregion product selection product name unit tests

        #region product selection sold out unit tests

        [Test] //Test if product selection cola is sold out 
        void TestIfProductSelectionColaSoldOut()
        {
            bool inStock = false;
            vm.items = new Dictionary<string, int>();
            string serialPortInputData = "<data><selection><cola>";
            comm.serialPortReceivedInput(serialPortInputData, null);
            IProduct product = new Product(serialPortInputData.Substring(18, 4), 0.0);

            if(vm.items.ContainsKey("cola") && vm.items["cola"] >= 0 )
                    inStock = true;

            Assert.IsFalse(inStock, String.Format("The item {0} is in stock.", product.Name));
        }

        [Test] //Test if product selection chips is sold out 
        void TestIfProductSelectionChipsSoldOut()
        {
            bool inStock = false;
            vm.items = new Dictionary<string, int>();
            string serialPortInputData = "<data><selection><chips>";
            comm.serialPortReceivedInput(serialPortInputData, null);
            IProduct product = new Product(serialPortInputData.Substring(18, 5), 0.0);

            if (vm.items.ContainsKey("chips") && vm.items["chips"] >= 0)
                inStock = true;

            Assert.IsFalse(inStock, String.Format("The item {0} is in stock.", product.Name));
        }

        [Test] //Test if product selection cola is sold out 
        void TestIfProductSelectionCandySoldOut()
        {
            bool inStock = false;
            vm.items = new Dictionary<string, int>();
            string serialPortInputData = "<data><selection><candy>";
            comm.serialPortReceivedInput(serialPortInputData, null);
            IProduct product = new Product(serialPortInputData.Substring(18, 5), 0.0);

            if (vm.items.ContainsKey("candy") && vm.items["candy"] >= 0)
                inStock = true;

            Assert.IsFalse(inStock, String.Format("The item {0} is in stock.", product.Name));
        }

        #endregion product selection sold out unit tests

        #region product selection display seen sold out unit tests

        [Test] //Test that display shows 'Sold Out' for the first time
        void TestDisplaySoldOutFirstTime()
        {
            selection = new Selection(new Product("", 0.0), 0.0);
            selection.SoldOut = true;
            selection.SeenSoldOut = false;
            string display = vm.checkDisplay();

            Assert.AreEqual(display, vm.message[3], "The display did not show 'Sold Out' as expected");
        }

        [Test] //Test that display shows 'Insert Coin' for the second and subsequent times
        void TestDisplaySoldOutSubsequentTimes()
        {
            selection = new Selection(new Product("", 0.0), 0.0);
            selection.SoldOut = true;
            selection.SeenSoldOut = true;
            string display = vm.checkDisplay();

            Assert.AreEqual(display, vm.message[0], "The display did not show 'Insert Coin' as expected");
        }

        #endregion product selection display seen sold out unit tests

        #region product selection display coin input unit tests

        [Test] //Test display shows the coin amount inserted if amount is less than product price
        void TestDisplayCoinSumLessThanProductPrice()
        {
            product = new Product(null, 1.75);
            selection = new Selection (product, 0.25);
            selection.getBalance();

            Assert.AreEqual(vm.display, selection.Inserted.ToString(), "The display is not showing the inserted amount.");          
        }

        [Test] //Test display shows 'Thank You' if coin amount inserted equals product price
        void TestDisplayCoinSumEqualProductPrice()
        {
            product = new Product(null, 1.75);
            selection = new Selection(product, 1.75);
            selection.getBalance();

            Assert.AreEqual(vm.display, "THANK YOU!", "The display is not 'thank you' as expected.");

        }

        #endregion product selection display coin input unit tests

        #region product selection inventory decrement unit tests

        [Test] //Test product cola decremented in inventory after successful selection request
        void TestColaSelectionDecrement()
        {
            product = new Product("cola", vm.products[0].Price);
            vm.items.Add("cola", 1);
            selection = new Selection(product, vm.products[0].Price);
            vm.decrementInventory(product);

            Assert.AreEqual(0, vm.items["cola"], String.Format("The inventory failed to decrement product {0}", product.Name));
        }

        [Test] //Test product chips decremented in inventory after successful selection request
        void TestChipsSelectionDecrement()
        {
            product = new Product("chips", vm.products[1].Price);
            vm.items.Add("chips", 1);
            selection = new Selection(product, vm.products[1].Price);
            vm.decrementInventory(product);

            Assert.AreEqual(0, vm.items["chips"], String.Format("The inventory failed to decrement product {0}", product.Name));
        }
        [Test] //Test product candy decremented in inventory after successful selection request
        void TestCandySelectionDecrement()
        {
            product = new Product("candy", vm.products[2].Price);
            vm.items.Add("candy", 1);
            selection = new Selection(product, vm.products[2].Price);
            vm.decrementInventory(product);

            Assert.AreEqual(0, vm.items["candy"], String.Format("The inventory failed to decrement product {0}", product.Name));
        }

        #endregion product selection inventory decrement unit tests

        #region product selection product ejection unit tests

        [Test] //Test product cola ejected after successful selection request
        void TestColaSelectionEject()
        {
            product = new Product("cola", vm.products[0].Price);
            comm.ejectProduct(product);

            Assert.IsTrue(comm.producEjected, String.Format("Failed to send eject product {0} to serial port.", product.Name));
        }

        [Test] //Test product chips ejected after successful selection request
        void TestChipsSelectionEject()
        {
            product = new Product("chips", vm.products[1].Price);
            comm.ejectProduct(product);

            Assert.IsTrue(comm.producEjected, String.Format("Failed to send eject product {0} to serial port.", product.Name));
        }

        [Test] //Test product candy ejected after successful selection request
        void TestCandySelectionEject()
        {
            product = new Product("candy", vm.products[2].Price);
            comm.ejectProduct(product);

            Assert.IsTrue(comm.producEjected, String.Format("Failed to send eject product {0} to serial port.", product.Name));
        }

        #endregion product selection product ejection unit tests

        #region product selection eject balance coin amount unit tests

        [Test] //Test balance coin amount ejected after successful cola selection request
        void TestEjectColaSelectionBalanceAmount()
        {
            product = new Product("cola", vm.products[0].Price);
            selection = new Selection(product, 3.00);
            if (selection.Inserted > product.Price)
                comm.ejectBalance(product.Price, selection.Inserted);

            Assert.IsTrue(comm.BalanceEjected, String.Format("Failed to eject balance for product selection: {0}", product.Name));
        }

        [Test] //Test balance coin amount ejected after successful chips selection request
        void TestEjectChipsSelectionBalanceAmount()
        {
            product = new Product("chips", vm.products[1].Price);
            selection = new Selection(product, 3.00);
            if (selection.Inserted > product.Price)
                comm.ejectBalance(product.Price, selection.Inserted);

            Assert.IsTrue(comm.BalanceEjected, String.Format("Failed to eject balance for product selection: {0}", product.Name));
        }

        [Test] //Test balance coin amount ejected after successful candy selection request
        void TestEjectCandySelectionBalanceAmount()
        {
            product = new Product("candy", vm.products[0].Price);
            selection = new Selection(product, 3.00);
            if (selection.Inserted > product.Price)
                comm.ejectBalance(product.Price, selection.Inserted);

            Assert.IsTrue(comm.BalanceEjected, String.Format("Failed to eject balance for product selection: {0}", product.Name));
        }

        #endregion product selection eject balance coin amount unit tests

        #endregion product selection unit tests
        
        #region coin-return unit tests

        [Test] //if the data received on serial port is a coin-return request
        void TestCoinReturnSignal()
        {
            comm.serialPortReceivedInput("<data><coinreturn>", null);
            Assert.AreEqual(3, comm.Signal);
        }

        [Test] //Test if 'return coins' command is sent successfully to the serial port
        void TestReturnCoinsCommandSuccess()
        {
            comm.returnCoins();
            Assert.IsTrue(comm.AllCoinsEjected, "Failed to return coins as expected.");
        }

        [Test] //Test that the display goes back to showing 'Insert Coin'
        void TestDisplayReturnInitialState()
        {
            vm.changeDisplay(vm.message[0]);
            Assert.AreEqual("INSERT COIN", vm.message[0], "Failed to return to display initial state 'Insert coin'.");
        }

        #endregion coin-return unit tests

        #region check reserves unit tests

        [Test] //Test 'check reserves' command sent successfully to serial port
        void TestCheckReservesCommandSuccess()
        {
            comm.checkReserves();
            Assert.IsTrue(comm.CheckReserves, "Failed to send 'check reserves' command to serial port as expected.");
        }

        [Test] //if the data received on serial port is a coin insertion
        void TestCheckReservesSignal()
        {
            comm.serialPortReceivedInput("<data><reserves>", null);
            Assert.AreEqual(4, comm.Signal);
        }

        [Test] //Test if low reserves
        void TestLowReserves()
        {
            string serialPortInputData = "<data><reserves><low>";
            comm.serialPortReceivedInput(serialPortInputData, null);
            string reserveLevel = serialPortInputData.Substring(17, serialPortInputData.IndexOf('>')-1);
            vm.ascertainReserves(reserveLevel);
            Assert.IsFalse(comm.Reserves, "Failed to ascertain low reserves as expected");
        }

        [Test] //Test if low reserves show 'Exact Change'
        void TestLowReservesDisplayExactChange()
        {
            string serialPortInputData = "<data><reserves><low>";
            comm.serialPortReceivedInput(serialPortInputData, null);
            string reserveLevel = serialPortInputData.Substring(17, serialPortInputData.IndexOf('>') - 1);
            vm.ascertainReserves(reserveLevel);
            vm.checkDisplay();
            Assert.AreEqual("EXACT CHANGE!", vm.display, "Failed to display 'Exact Change' as expected.");
        }

        [Test] //Test if high reserves
        void TestHighReserves()
        {
            string serialPortInputData = "<data><reserves><high>";
            comm.serialPortReceivedInput(serialPortInputData, null);
            string reserveLevel = serialPortInputData.Substring(17, serialPortInputData.IndexOf('>') - 1);
            vm.ascertainReserves(reserveLevel);
            Assert.IsTrue(comm.Reserves, "Failed to ascertain high reserves as expected");
        }

        #endregion check reserves unit tests

        #endregion check communication on serial port
    }

    #endregion VendingMachineApp unit tests

}
