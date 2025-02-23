<template>
  <div class="accounts_table">
    <div v-if="loading" class="loading">
      Loading...
    </div>

    <div v-if="!loading">
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

      <div>
        <label v-if="selectedAccount">Currently selected account: {{ selectedAccount.accountName }}</label>
        <br />
        <label v-if="selectedUser">Currently selected user: {{ selectedUser.userName }} - {{ selectedUser.phoneNumber }}</label>
      </div>
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
              selectedUser: null
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
