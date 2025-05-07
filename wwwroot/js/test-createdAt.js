// Test script for CreatedAt functionality
document.addEventListener('DOMContentLoaded', function() {
    console.log('CreatedAt test script loaded');
    
    // Function to test if tables are sorted by CreatedAt
    function testSortedByCreatedAt() {
        // For Equipment table
        const equipmentTable = document.querySelector('#equipmentTable');
        if (equipmentTable) {
            console.log('Testing Equipment table sorting');
            const rows = Array.from(equipmentTable.querySelectorAll('tbody tr'));
            
            // We don't have direct access to CreatedAt in the DOM, but we can log the order
            console.log('Equipment items order:', rows.map(row => row.querySelector('td').textContent.trim()));
            console.log('Equipment table found with ' + rows.length + ' items');
        }
        
        // For Vendors table
        const vendorsTable = document.querySelector('.table-hover');
        if (vendorsTable && document.title.includes('Vendors')) {
            console.log('Testing Vendors table sorting');
            const rows = Array.from(vendorsTable.querySelectorAll('tbody tr'));
            
            console.log('Vendors order:', rows.map(row => row.querySelector('td').textContent.trim()));
            console.log('Vendors table found with ' + rows.length + ' items');
        }
    }
    
    // Run the test with a slight delay to ensure page is fully loaded
    setTimeout(testSortedByCreatedAt, 500);
    
    // Log to confirm GitHub button styling is applied
    const githubButton = document.querySelector('.github-button');
    if (githubButton) {
        console.log('GitHub button found with text:', githubButton.textContent.trim());
        console.log('GitHub button styles:', {
            display: window.getComputedStyle(githubButton).display,
            borderRadius: window.getComputedStyle(githubButton).borderRadius,
            padding: window.getComputedStyle(githubButton).padding
        });
    }
    
    // Log to confirm particles.js animation is applied
    const particlesCanvas = document.querySelector('#particles-js canvas');
    if (particlesCanvas) {
        console.log('Particles.js canvas found with dimensions:', {
            width: particlesCanvas.width,
            height: particlesCanvas.height
        });
    }
}); 