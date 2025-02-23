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
        <br />
        <label v-if="selectedUser">Currently selected user: {{ selectedUser.userName }} - {{ selectedUser.phoneNumber }}</label>
      </div>
    </div>

    
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
              newUserNumber: ""
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
          },
          async addAccount() {
            if (!this.newAccountName || this.newAccountName.length === 0) {
              return;
            }
            await this.postRequest('gatekeeper/addaccount', JSON.stringify(this.newAccountName));
            this.newAccountName = "";
            await this.fetchData();
          },
          async postRequest(uri, data) {
            const requestOptions = {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: data
            };

            await fetch(uri, requestOptions);
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
