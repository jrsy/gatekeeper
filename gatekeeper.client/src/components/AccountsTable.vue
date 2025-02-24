<template>
  <div class="accounts_table">
    <div v-if="loading" class="loading">
      Loading...
    </div>

    <div v-if="!loading">
      <div>
        <input v-model="newAccountName" placeholder="New Account Name"></input>
        <button @click="addAccount">Add New Account</button>
      </div>

      <div v-if="accounts" class="tableRow">
        <div class="tableColumn">
          <table id="accountTable">
            <thead>Accounts</thead>
            <tbody>
              <tr v-for="account in accounts" :key="account.accountId" @click="selectAccount(account.accountId)">
                <td>{{ account.accountName }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="tableColumn">
          <table id="userTable">
            <thead>Users</thead>
            <tbody>
              <tr v-for="user in users" :key="user.userId" @click="selectUser(user.userId)">
                <td>{{ user.userName }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div id="userInfo">
        <input v-model="newUserName" placeholder="New User Name"></input>
        <input v-model="newUserNumber" placeholder="New User Number"></input>
      </div>
      <button v-if="selectedAccount" @click="addUser">Add User to Account</button>
    </div>

    <div>
      <label v-if="selectedAccount">Currently selected account: {{ selectedAccount.accountName }}</label>
      <div v-if="selectedUser">
        <br />
        <label v-if="selectedUser">Currently selected user: {{ selectedUser.userName }} - {{ selectedUser.phoneNumber }}</label>
        <br />
        <input v-model="newUserMessage" placeholder="New User Message"></input>
        <br />
        <button v-if="showSendButtons()" @click="addMessages(10)">Add message to queue 10 times</button>
        <button v-if="showSendButtons()" @click="addMessages(100)">Add message to queue 100 times</button>
        <button v-if="showSendButtons()" @click="addMessages(1000)">Add message to queue 1000 times</button>
        <button v-if="showSendButtons()" @click="addMessages(10000)">Add message to queue 10000 times</button>
      </div>
    </div>
  </div>

  <div v-if="accounts" class="tableRow">
    <div class="tableColumn">
      <table id="pendingTable">
        <thead>Pending Messages</thead>
        <tbody>
          <tr v-for="message in pendingMessages">
            <td>{{ message.accountName }}</td>
            <td>{{ message.userName }}</td>
            <td>{{ message.Data }}</td>
          </tr>
        </tbody>
      </table>
    </div>
    <div class="tableColumn">
      <table id="sentTable">
        <thead>Sent Messages</thead>
        <tbody>
          <tr v-for="message in sentMessages">
            <td>{{ message.response }}</td>
            <td>{{ message.userName }}</td>
            <td>{{ message.timestamp }}</td>
            <td>{{ message.messageId }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <button v-if="showSendMessages()" @click="sendMessages">Send all messages</button>
</template>

<script lang="js">
    import { defineComponent } from 'vue';

    export default defineComponent({
        data() {
            return {
              loading: false,
              accounts: [],
              users: [],
              selectedAccount: null,
              selectedUser: null,
              newAccountName: "",
              newUserName: "",
              newUserNumber: "",
              newUserMessage: "",
              pendingMessages: [],
              sentMessages: []
            };
        },
        async created() {
            await this.fetchData();
        },
        watch: {
            // call again the method if the route changes
            '$route': 'fetchData'
        },
        methods: {
          async fetchData() {

            this.accounts = [];
            this.loading = true;

            var response = await fetch('gatekeeper');
            if (response.ok) {
              this.accounts = await response.json();
              this.loading = false;
            }
          },
          showSendButtons() {
            return this.newUserMessage.length > 0;
          },
          showSendMessages() {
            return this.pendingMessages.length > 0;
          },
          selectAccount(accountId) {
            this.selectedAccount = this.accounts.find(a => a.accountId === accountId);
            this.users = this.selectedAccount.users;
            this.selectedUser = null;
          },
          selectUser(userId) {
            this.selectedUser = this.users.find(u => u.userId === userId);
          },
          async addUser() {
            if (!this.selectedAccount || this.newUserName.length === 0 || this.newUserNumber === 0) {
              return;
            }

            await this.postRequest('gatekeeper/adduser', JSON.stringify({
              accountId: this.selectedAccount.accountId,
              userName: this.newUserName,
              phoneNumber: this.newUserNumber
            }));

            this.newUserName = "";
            this.newUserNumber = "";
            await this.fetchData();

            this.selectAccount(this.selectedAccount.accountId);
          },
          async addAccount() {
            if (!this.newAccountName || this.newAccountName.length === 0) {
              return;
            }
            await this.postRequest('gatekeeper/addaccount', JSON.stringify(this.newAccountName));
            this.newAccountName = "";
            await this.fetchData();
            this.selectedAccount = null;
          },
          addMessages(no_of_msgs) {
            var accountId = this.selectedAccount.accountId;
            var userId = this.selectedUser.userId;
            var message = {
              AccountId: accountId,
              UserId: userId,
              Data: this.newUserMessage,
              SendImmediately: true,
              accountName: this.selectedAccount.accountName,
              userName: this.selectedUser.userName,
            };

            for (var i = 0; i < no_of_msgs; i++) {
              this.pendingMessages.push(message);
            }

            this.newUserMessage = "";
          },
          async sendMessages() {
            this.pendingMessages.forEach(async message => {
              var response = await this.postRequest('gatekeeper/sendmessage', JSON.stringify({
                message: message
              }));
              var result = await response.json();
              var sentMessage = {
                accountName: message.accountName,
                userName: message.userName,
                response: result.response,
                messageId: result.messageId,
                timestamp: new Date(result.timestamp).toLocaleString()
              }
              this.sentMessages.push(sentMessage);
            });

            this.pendingMessages = [];
          },
          async postRequest(uri, data) {
            const requestOptions = {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: data
            };

            var response = await fetch(uri, requestOptions);
            if (response.ok) {
              return response;
              
            }
          }
        },
    });
</script>

<style scoped>
  table {
    font-family: Arial, Helvetica, sans-serif;
    border-collapse: collapse;
    width: 100%;
    padding: 5px;
    display: table;
  }

  thead {
    padding-top: 12px;
    padding-bottom: 12px;
    text-align: left;
    background-color: #505050;
    color: white;
  }

  td {
    border: 1px solid #ddd;
    padding: 8px;
    height: 50px;
    width: 100%;
  }

  .tableRow {
    margin-left: -5px;
    margin-right: -5px;
  }

  .tableColumn {
    float: left;
    width: 50%;
    height: 200px;
    padding: 5px;
    overflow-y: scroll;
  }

  tr:nth-child(even) {
    background-color: white;
  }

  tr:hover {
    background-color: #b6ff00;
  }

  
</style>
