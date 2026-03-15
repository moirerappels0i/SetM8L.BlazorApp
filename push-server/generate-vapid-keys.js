/**
 * VAPID Key Generator for M8L Set Card Game
 *
 * Run this script once to generate your VAPID key pair:
 *   node generate-vapid-keys.js
 *
 * Or use the npm script:
 *   npm run generate-vapid-keys
 *
 * After generating:
 * 1. Put the PUBLIC key in:
 *    - PushNotificationService.cs (VapidPublicKey constant)
 *    - Environment variable VAPID_PUBLIC_KEY for the push server
 *
 * 2. Put the PRIVATE key in:
 *    - Environment variable VAPID_PRIVATE_KEY for the push server
 *    - NEVER put the private key in client-side code
 */

const webpush = require('web-push');

const vapidKeys = webpush.generateVAPIDKeys();

console.log('=== VAPID Keys Generated ===\n');
console.log('Public Key (put in client code & server env):');
console.log(vapidKeys.publicKey);
console.log('\nPrivate Key (server-side ONLY, keep secret):');
console.log(vapidKeys.privateKey);
console.log('\n=== Environment Variables ===\n');
console.log(`VAPID_PUBLIC_KEY=${vapidKeys.publicKey}`);
console.log(`VAPID_PRIVATE_KEY=${vapidKeys.privateKey}`);
console.log(`VAPID_EMAIL=mailto:your-email@example.com`);
console.log('\n=== For PushNotificationService.cs ===\n');
console.log(`private const string VapidPublicKey = "${vapidKeys.publicKey}";`);
