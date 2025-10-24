from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager

# --- CONFIG ---
PAGE_URL = "https://susnet.co.za"

# --- SELENIUM SETUP ---
options = Options()
options.add_argument("--start-maximized")

driver = webdriver.Chrome(service=Service(ChromeDriverManager().install()), options=options)
wait = WebDriverWait(driver, 10)  # wait up to 10 seconds for elements

try:
    driver.get(PAGE_URL)

    # 1. Wait for Navbar
    navbar = wait.until(EC.presence_of_element_located((By.TAG_NAME, "nav")))
    assert navbar.is_displayed(), "Navbar not visible"

    # 2. Check Nav Links
    expected_nav_links = ["Dashboard", "Revenue Page", "Stock", "Phones", "Logs"]
    nav_links = wait.until(EC.presence_of_all_elements_located((By.CSS_SELECTOR, "a.nav-link")))
    nav_texts = [link.text.strip() for link in nav_links]
    for text in expected_nav_links:
        assert text in nav_texts, f"Missing nav link: {text}"

    # 3. Check Dashboard Tiles
    expected_tiles = ["Total Orders", "Pending Orders", "Completed Orders", "Materials Ready"]
    for tile in expected_tiles:
        el = wait.until(
            EC.presence_of_element_located((By.XPATH, f"//*[contains(text(), '{tile}')]"))
        )
        assert el.is_displayed(), f"Dashboard tile missing: {tile}"

    # 4. Check Material Inventory Items
    expected_materials = ["Copper", "Silicon", "Sand", "Plastic", "Aluminum"]
    for material in expected_materials:
        el = wait.until(
            EC.presence_of_element_located((By.XPATH, f"//*[contains(text(), '{material}')]"))
        )
        assert el.is_displayed(), f"Material missing: {material}"

    print("✅ All UI checks passed — page looks good!")

finally:
    driver.quit()
