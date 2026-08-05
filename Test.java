// Java Program to Login to a specific Webpage
// Using Selenium WebDriver and ChromeDriver

package login_test;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;

public class Test {

    public static void main(String[] args)
    {
        WebDriver driver = new ChromeDriver();

        // URL of the login website
        driver.get("http://myladot.lacity.org/eWork/Account/LogOn");

        // Maximize window size
        driver.manage().window().maximize();

        // Updated locator using the id="UserName" from the HTML inspect element
        driver.findElement(By.id("UserName"))
            .sendKeys("ckim");

        // Enter your login password
        driver.findElement(By.id("Password"))
            .sendKeys("18742361!Ladot");

        //Locate by input value and click
        driver.findElement(By.xpath("//input[@value='Log On']"))
            .click();
    }
}
