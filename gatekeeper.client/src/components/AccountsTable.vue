<template>
  <div class="accounts_table">
    <h1>GateKeeper</h1>

    <div v-if="loading" class="loading">
      Loading...
    </div>

    <div v-if="accounts" class="content">
      <table>
        <tbody>
          <tr v-for="account in accounts" :key="account.accountId" @click="selectAccount(account.accountId)">
            <td>{{ account.accountName }}</td>
          </tr>
        </tbody>
      </table>
      <table>
        <tbody>
          <tr v-for="user in users" :key="user.userId">
            <td>{{ user.userName }}</td>
          </tr>
        </tbody>
      </table>
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
            var selectedAccount = this.accounts.find(a => a.accountId === accountId);
            this.users = selectedAccount.users;
          }
        },
    });
</script>

<style scoped>
  th {
    font-weight: bold;
  }

  th, td {
    padding-left: .5rem;
    padding-right: .5rem;
  }

  .weather-component {
    text-align: center;
  }

  table {
    margin-left: auto;
    margin-right: auto;
  }
</style>
